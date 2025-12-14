#!/bin/bash
# entrypoint.sh

/opt/mssql/bin/sqlservr &

if [ -z "$MSSQL_SA_PASSWORD" ]; then
  echo "❌ MSSQL_SA_PASSWORD não está definida."
  exit 1
fi

echo "⏳ waiting for SQL Server to start..."

MAX_RETRIES=15

for i in $(seq 1 15); do
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -N -C -Q "SELECT 1" &> /dev/null
    if [ $? -eq 0 ]; then
        echo "✅ SQL Server ready!"
        break
    fi
    echo "⏳ Trying to connect ($i)..."
    sleep 2
done

if [ $i -eq $MAX_RETRIES ]; then
    echo "❌ Failed to connect to SQL Server after multiple attempts."
    exit 1
fi

echo "🚀 Running startup script..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -N -C -i /scripts/SqlServer_script.sql

echo "🚀 Running tuning script..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -N -C -i /scripts/tuning_script.sql

wait