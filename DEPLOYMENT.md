# Despliegue

## 1. Local con Docker Compose

Levanta la API + PostgreSQL con el esquema Database First ya aplicado:

```bash
docker compose up --build
```

- API / Swagger: http://localhost:8080/swagger
- PostgreSQL: `localhost:5432` (db `ceiba_reservations`, user/pass `postgres`/`postgres`)

El contenedor de Postgres ejecuta `db/docker/initdb.sh`, que aplica en orden los
scripts `db/scripts/01..12` (omite el `00`, que crea la base). Esto solo ocurre en
el **primer** arranque (cuando el volumen `pgdata` está vacío); para reaplicar el
esquema desde cero:

```bash
docker compose down -v && docker compose up --build
```

## 2. CI (GitHub Actions)

`.github/workflows/ci.yml` se ejecuta en cada push/PR a `master`: restore, build y
test de toda la solución. Las pruebas de integración usan **Testcontainers**, que
funciona en los runners `ubuntu-latest` (Docker preinstalado) sin configuración extra.

## 3. CD a Google Cloud Run

Arquitectura objetivo:

```
GitHub Actions ──build/push──> Artifact Registry ──deploy──> Cloud Run ──Unix socket──> Cloud SQL (PostgreSQL)
```

`.github/workflows/cd.yml` construye la imagen, la publica en Artifact Registry y
despliega en Cloud Run. La cadena de conexión se inyecta desde **Secret Manager**.

### 3.1 Infraestructura GCP (una sola vez)

Sustituye `PROJECT_ID` y `REGION` (p. ej. `us-central1`).

```bash
gcloud config set project PROJECT_ID

# Habilitar APIs
gcloud services enable run.googleapis.com artifactregistry.googleapis.com \
    sqladmin.googleapis.com secretmanager.googleapis.com iamcredentials.googleapis.com

# Artifact Registry (Docker)
gcloud artifacts repositories create reservations \
    --repository-format=docker --location=REGION

# Cloud SQL (PostgreSQL)
gcloud sql instances create reservations-sql \
    --database-version=POSTGRES_16 --tier=db-f1-micro --region=REGION
gcloud sql databases create ceiba_reservations --instance=reservations-sql
gcloud sql users set-password postgres --instance=reservations-sql --password='UNA_CLAVE_FUERTE'
```

### 3.2 Aplicar el esquema sobre Cloud SQL (Database First)

Con el [Cloud SQL Auth Proxy](https://cloud.google.com/sql/docs/postgres/connect-auth-proxy)
en una terminal:

```bash
cloud-sql-proxy PROJECT_ID:REGION:reservations-sql --port 5432
```

En otra terminal, aplica los scripts `01..12` en orden (el `00` no hace falta, la base
ya existe):

```bash
for f in $(ls db/scripts/*.sql | sort); do
  case "$(basename "$f")" in 00_*) continue;; esac
  PGPASSWORD='UNA_CLAVE_FUERTE' psql -h 127.0.0.1 -U postgres -d ceiba_reservations -v ON_ERROR_STOP=1 -f "$f"
done
```

### 3.3 Secret Manager: cadena de conexión

Cloud Run se conecta a Cloud SQL por **socket Unix** `/cloudsql/<CONNECTION_NAME>`.
El `CONNECTION_NAME` es `PROJECT_ID:REGION:reservations-sql`.

```bash
printf 'Host=/cloudsql/PROJECT_ID:REGION:reservations-sql;Database=ceiba_reservations;Username=postgres;Password=UNA_CLAVE_FUERTE' \
  | gcloud secrets create reservations-db-connstring --data-file=-
```

### 3.4 Identidad para GitHub Actions (Workload Identity Federation)

Recomendado frente a claves de cuenta de servicio (sin secretos de larga duración).

```bash
# Cuenta de servicio del despliegue
gcloud iam service-accounts create gh-deployer --display-name="GitHub Actions Deployer"
SA="gh-deployer@PROJECT_ID.iam.gserviceaccount.com"

# Permisos mínimos
for role in roles/run.admin roles/artifactregistry.writer \
            roles/cloudsql.client roles/secretmanager.secretAccessor \
            roles/iam.serviceAccountUser; do
  gcloud projects add-iam-policy-binding PROJECT_ID --member="serviceAccount:$SA" --role="$role"
done

# Pool + proveedor OIDC para GitHub
gcloud iam workload-identity-pools create github --location=global --display-name="GitHub"
gcloud iam workload-identity-pools providers create-oidc github-provider \
  --location=global --workload-identity-pool=github \
  --display-name="GitHub provider" \
  --attribute-mapping="google.subject=assertion.sub,attribute.repository=assertion.repository" \
  --attribute-condition="assertion.repository=='OWNER/REPO'" \
  --issuer-uri="https://token.actions.githubusercontent.com"

# Permite que el repo impersone la cuenta de servicio
PROJECT_NUMBER=$(gcloud projects describe PROJECT_ID --format='value(projectNumber)')
gcloud iam service-accounts add-iam-policy-binding "$SA" \
  --role=roles/iam.workloadIdentityUser \
  --member="principalSet://iam.googleapis.com/projects/$PROJECT_NUMBER/locations/global/workloadIdentityPools/github/attribute.repository/OWNER/REPO"
```

### 3.5 Secretos y variables del repositorio (GitHub)

En **Settings → Secrets and variables → Actions**:

| Tipo      | Nombre                      | Valor                                                                 |
|-----------|-----------------------------|-----------------------------------------------------------------------|
| Variable  | `GCP_PROJECT_ID`            | `PROJECT_ID`                                                          |
| Variable  | `GCP_REGION`                | `REGION` (p. ej. `us-central1`)                                       |
| Secret    | `GCP_WIF_PROVIDER`          | `projects/PROJECT_NUMBER/locations/global/workloadIdentityPools/github/providers/github-provider` |
| Secret    | `GCP_SERVICE_ACCOUNT`       | `gh-deployer@PROJECT_ID.iam.gserviceaccount.com`                     |
| Secret    | `CLOUD_SQL_CONNECTION_NAME` | `PROJECT_ID:REGION:reservations-sql`                                 |

Con esto, cada push a `master` despliega automáticamente. También puedes lanzarlo a
mano desde la pestaña **Actions → CD → Run workflow**.

### 3.6 Job diario RN-06 (Cloud Scheduler → endpoint)

La API trae un `BackgroundService` que ejecuta RN-06 cada 24 h, pero **Cloud Run escala a
cero**, por lo que ese temporizador no corre de forma fiable. La opción robusta en GCP es
**Cloud Scheduler** llamando a diario al endpoint, que ejecuta la misma lógica:

```
POST https://<URL-de-Cloud-Run>/api/event/complete-finished
```

La autenticación se hace con un **token OIDC** que emite Cloud Scheduler con una cuenta de
servicio dedicada.

```bash
gcloud services enable cloudscheduler.googleapis.com

# Cuenta de servicio que usará el scheduler para invocar Cloud Run
gcloud iam service-accounts create scheduler-invoker --display-name="Cloud Scheduler Invoker"
SCHED_SA="scheduler-invoker@PROJECT_ID.iam.gserviceaccount.com"

# Permiso para invocar el servicio de Cloud Run
gcloud run services add-iam-policy-binding reservations-api \
  --region=REGION \
  --member="serviceAccount:$SCHED_SA" \
  --role=roles/run.invoker

# URL del servicio (audience del token OIDC)
RUN_URL=$(gcloud run services describe reservations-api --region=REGION --format='value(status.url)')

# Job diario (03:00, zona horaria de Bogotá). Cron: min hora * * *
gcloud scheduler jobs create http complete-finished-events \
  --location=REGION \
  --schedule="0 3 * * *" \
  --time-zone="America/Bogota" \
  --uri="$RUN_URL/api/event/complete-finished" \
  --http-method=POST \
  --oidc-service-account-email="$SCHED_SA" \
  --oidc-token-audience="$RUN_URL"
```

> **Sobre la autenticación.** Hoy el servicio se despliega con `--allow-unauthenticated`
> (API pública), así que Cloud Run **no rechaza** las llamadas sin token; el OIDC anterior
> identifica al llamador pero no lo exige. Si quieres exigirlo de verdad:
> - **Opción A (recomendada si la API no es pública):** redepliega sin
>   `--allow-unauthenticated` y concede `roles/run.invoker` solo a los clientes legítimos
>   (incluida `scheduler-invoker`). Cloud Run rechazará todo lo no autenticado.
> - **Opción B (API pública pero endpoint protegido):** mantén la API pública y protege solo
>   este endpoint en la app (p. ej. validando una cabecera/clave que envíe el scheduler).
>   Requiere un pequeño cambio en `CompleteFinished` para validar la credencial.

Prueba manual del job: `gcloud scheduler jobs run complete-finished-events --location=REGION`.

**Automatización opcional en el CD.** El workflow `cd.yml` incluye un paso idempotente que
crea/actualiza este job tras cada despliegue. Está **desactivado por defecto**; para
activarlo define en GitHub la variable `SETUP_SCHEDULER = true`. La cuenta de servicio del
scheduler debe existir previamente (`scheduler-invoker@PROJECT_ID...`) con `roles/run.invoker`.
Variables opcionales: `SCHEDULER_CRON` (def. `0 3 * * *`) y `SCHEDULER_TIMEZONE`
(def. `America/Bogota`).

### Alternativas

- **GKE / App Engine flexible**: posibles, pero Cloud Run es el encaje natural para un
  contenedor stateless como esta API.
- **Clave de cuenta de servicio** en lugar de WIF: más simple pero menos seguro; usarías
  `google-github-actions/auth@v2` con `credentials_json` desde un secreto.
