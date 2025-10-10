#!/bin/bash
set -e

DB_USER="postgres"
DB_NAME="api"

psql -v ON_ERROR_STOP=1 -U "$DB_USER" -d postgres <<-EOSQL
  SELECT 'CREATE DATABASE ${DB_NAME}'
  WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = '${DB_NAME}'
  )\gexec
EOSQL

psql -v ON_ERROR_STOP=1 -U "$DB_USER" -d "$DB_NAME" <<-EOSQL
-- 🔧 Conexões e cache
  ALTER SYSTEM SET max_connections = '${POSTGRES_MAX_CONNECTIONS:-300}';
  ALTER SYSTEM SET shared_buffers = '${POSTGRES_SHARED_BUFFERS:-1GB}'; -- ~25% total RAM
  ALTER SYSTEM SET effective_cache_size = '${POSTGRES_EFFECTIVE_CACHE_SIZE:-3GB}'; -- ~75% total RAM

  -- 🧮 Memória de workspace
  ALTER SYSTEM SET work_mem = '${POSTGRES_WORK_MEM:-8MB}';
  ALTER SYSTEM SET maintenance_work_mem = '256MB';

  -- 🧾 WAL e Checkpoints
  ALTER SYSTEM SET wal_buffers = '8MB';
  ALTER SYSTEM SET max_wal_size = '1GB';
  ALTER SYSTEM SET min_wal_size = '256MB';
  ALTER SYSTEM SET checkpoint_timeout = '10min';
  ALTER SYSTEM SET checkpoint_completion_target = '0.9';
  ALTER SYSTEM SET synchronous_commit = '${POSTGRES_SYNCHRONOUS_COMMIT:-off}';

  -- ⚙️ Planejador e paralelismo
  ALTER SYSTEM SET random_page_cost = 1.1;
  ALTER SYSTEM SET seq_page_cost = 1.0;
  ALTER SYSTEM SET effective_io_concurrency = 200;
  ALTER SYSTEM SET max_parallel_workers_per_gather = 2;

  -- 🧹 Autovacuum e background writer
  ALTER SYSTEM SET autovacuum = on;
  ALTER SYSTEM SET autovacuum_max_workers = 3;
  ALTER SYSTEM SET autovacuum_naptime = '45s';
  ALTER SYSTEM SET autovacuum_vacuum_scale_factor = 0.1;
  ALTER SYSTEM SET autovacuum_analyze_scale_factor = 0.05;
  ALTER SYSTEM SET bgwriter_lru_maxpages = 800;
  ALTER SYSTEM SET bgwriter_lru_multiplier = 3.0;

  -- 🌐 TCP keepalive e rede (melhora round-trip)
  ALTER SYSTEM SET tcp_keepalives_idle = 30;
  ALTER SYSTEM SET tcp_keepalives_interval = 10;
  ALTER SYSTEM SET tcp_keepalives_count = 3;

  -- 🪵 Logs e monitoramento
  ALTER SYSTEM SET log_min_duration_statement = '200ms';
  ALTER SYSTEM SET log_checkpoints = 'on';
  ALTER SYSTEM SET log_autovacuum_min_duration = 0;
  ALTER SYSTEM SET log_temp_files = 0;
  ALTER SYSTEM SET track_io_timing = on;
  ALTER SYSTEM SET track_activity_query_size = 2048;
EOSQL

pg_ctl -D "${PGDATA}" restart -m fast
