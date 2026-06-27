#!/bin/bash
# =============================================================================
# Aplica los scripts Database First sobre la base que crea el contenedor Postgres.
# Se ejecuta automáticamente (docker-entrypoint-initdb.d) en el primer arranque,
# cuando la base indicada por POSTGRES_DB ya existe; por eso se omite el 00.
# =============================================================================
set -euo pipefail

for script in $(ls /db-scripts/*.sql | sort); do
    case "$(basename "$script")" in
        00_*)
            echo "Omitiendo $script (la base ya existe)"
            continue
            ;;
    esac
    echo "Aplicando $script"
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -f "$script"
done
