
    create table Billing.BalanceOperations (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Type INTEGER default 0  not null,
       Description VARCHAR(255),
       Date DATETIME default 0  not null,
       Sum NUMERIC(19,5) default 0  not null,
       Payer INTEGER UNSIGNED,
       primary key (Id)
    );
