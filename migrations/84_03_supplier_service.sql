
    create table Future.Services (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Type INTEGER default 0  not null,
       Name VARCHAR(255),
       Disabled TINYINT(1) default 0  not null,
       HomeRegion BIGINT UNSIGNED,
       primary key (Id)
    );

    create table Future.Suppliers (
        Id INTEGER UNSIGNED not null,
       Name VARCHAR(255),
       FullName VARCHAR(255),
       RegionMask BIGINT UNSIGNED default 0  not null,
       Disabled TINYINT(1) default 0  not null,
       Registrant VARCHAR(255),
       RegistrationDate DATETIME,
       HomeRegion BIGINT UNSIGNED,
       ContactGroupOwnerId INTEGER UNSIGNED,
       primary key (Id)
    );
