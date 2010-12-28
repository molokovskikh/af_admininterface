
    create table billing.InvoiceParts (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Name VARCHAR(255),
       Cost NUMERIC(19,5),
       Count INTEGER,
       Invoice INTEGER UNSIGNED,
       primary key (Id)
    );
