    \c api;
    
    CREATE EXTENSION IF NOT EXISTS pg_trgm;

	CREATE TABLE IF NOT EXISTS Persons(
                        ID uuid NOT NULL,
                        Alias varchar(32) unique NOT NULL,
                        Name varchar(100) NOT NULL,
                        Birthdate timestamp NOT NULL,
                        Stack varchar(4000),
                        Search varchar(1160) generated always as (
                            LOWER(COALESCE(Alias,'') || ' ' || COALESCE(Name,'') || ' ' || COALESCE(Stack,''))
                        ) stored,
                        PRIMARY KEY (ID)
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_search_person on Persons USING gist (Search gist_trgm_ops);

                    CREATE INDEX IF NOT EXISTS idx_alias on Persons USING btree (Alias);


	CREATE TABLE IF NOT EXISTS Companies(
                        ID uuid NOT NULL,                        
                        TradeName varchar(100) NOT NULL,
	      BusinessSector varchar(32) unique NOT NULL,
	      NumberEmployees int,
                        FoundingDate timestamp NOT NULL,
                        ServiceProductCatalog varchar(4000),
                        Search varchar(1160) generated always as (
                            LOWER(COALESCE(TradeName,'') || ' ' || COALESCE(BusinessSector,'') || ' ' || COALESCE(ServiceProductCatalog,''))
                        ) stored,
                        PRIMARY KEY (ID)
                    );
                    
                    CREATE INDEX IF NOT EXISTS idx_search_companies on Companies USING gist (Search gist_trgm_ops);

                    CREATE INDEX IF NOT EXISTS idx_TradeName on Companies USING btree (TradeName);