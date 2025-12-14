-- Enable advanced options
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

-- Limit memory
EXEC sp_configure 'max server memory (MB)', 3574;
RECONFIGURE;

-- Parallelism tuning
EXEC sp_configure 'max degree of parallelism', 2;
RECONFIGURE;

EXEC sp_configure 'cost threshold for parallelism', 50;
RECONFIGURE;

-- TempDB optimization
ALTER DATABASE tempdb MODIFY FILE (NAME = tempdev, SIZE = 256MB, FILEGROWTH = 64MB);
ALTER DATABASE tempdb ADD FILE (NAME = tempdev2, FILENAME = '/var/opt/mssql/data/tempdb2.ndf', SIZE = 256MB, FILEGROWTH = 64MB);
GO

ALTER DATABASE tempdb MODIFY FILE (NAME = tempdev, MAXSIZE = 1GB);
ALTER DATABASE tempdb MODIFY FILE (NAME = tempdev2, MAXSIZE = 1GB);
GO

-- Recovery model SIMPLE
ALTER DATABASE [api] SET RECOVERY SIMPLE;
GO

-- Filegrowth optimization
ALTER DATABASE api
MODIFY FILE (NAME = api, FILEGROWTH = 256MB);
GO

ALTER DATABASE api
MODIFY FILE (NAME = api_log, FILEGROWTH = 256MB);
GO