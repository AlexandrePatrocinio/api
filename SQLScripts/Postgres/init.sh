#!/bin/bash
set -e

DB_USER="postgres"

psql -v ON_ERROR_STOP=1 -U "$DB_USER" -d postgres <<-EOSQL
  SELECT 'CREATE DATABASE api'
  WHERE NOT EXISTS (
    SELECT FROM pg_database WHERE datname = 'api'
  )\gexec
EOSQL