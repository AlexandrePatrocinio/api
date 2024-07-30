	create DATABASE api
    GO

    use api
    GO

    IF OBJECT_ID(N'dbo.Persons', N'U') IS NULL
	begin
        CREATE TABLE Persons (
            ID uniqueidentifier NOT NULL,
            Alias nvarchar(32) NOT NULL,
            Name varchar(100) NOT NULL,
            Birthdate datetime NOT NULL,
            Stack nvarchar(4000),
            Search AS LOWER(COALESCE(Alias,'') + ' ' + COALESCE(Name,'') + ' ' + COALESCE(Stack,'')) PERSISTED,
            PRIMARY KEY (ID)
        );

        IF OBJECT_ID(N'dbo.idx_search', N'IX') IS NULL
            CREATE INDEX idx_search ON Persons (Search);

        IF OBJECT_ID(N'dbo.idx_alias', N'IX') IS NULL
            CREATE INDEX idx_alias ON Persons (Alias);
	end


	IF OBJECT_ID(N'dbo.Companies', N'U') IS NULL
	begin
        CREATE TABLE Companies(
            ID uniqueidentifier NOT NULL,                        
            TradeName nvarchar(100) NOT NULL,
            BusinessSector nvarchar(32) unique NOT NULL,
            NumberEmployees int,
            FoundingDate datetime NOT NULL,
            ServiceProductCatalog nvarchar(4000),
            Search AS LOWER(
                COALESCE(TradeName,'') + ' ' + COALESCE(BusinessSector,'') + ' ' + COALESCE(ServiceProductCatalog,'')
            ) PERSISTED,
            PRIMARY KEY (ID)
        );

        IF OBJECT_ID(N'dbo.idx_search', N'IX') IS NULL
            CREATE INDEX idx_search ON Companies (Search);                   

        IF OBJECT_ID(N'dbo.idx_tradename', N'IX') IS NULL
            CREATE INDEX idx_tradename ON Companies (TradeName);
    end