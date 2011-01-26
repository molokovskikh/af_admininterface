
    create table Billing.ActParts (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Name VARCHAR(255),
       Cost NUMERIC(19,5),
       Count INTEGER,
       Act INTEGER UNSIGNED,
       primary key (Id)
    );

    create table Billing.Acts (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Period INTEGER,
       ActDate DATETIME,
       Recipient INTEGER UNSIGNED,
       Payer INTEGER UNSIGNED,
       primary key (Id)
    );
