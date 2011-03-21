
    create table Future.Services (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       primary key (Id)
    );

    create table Future.Suppliers (
        SupplierId INTEGER UNSIGNED not null,
       Name VARCHAR(255),
       FullName VARCHAR(255),
       Registrant VARCHAR(255),
       RegistrationDate DATETIME,
       HomeRegion BIGINT UNSIGNED,
       ContactGroupOwnerId INTEGER UNSIGNED,
       primary key (SupplierId)
    );
