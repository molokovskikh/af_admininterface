
    create table Billing.Advertisings (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Begin DATETIME,
       End DATETIME,
       Cost NUMERIC(19,5),
       PayedSum NUMERIC(19,5),
       Payer INTEGER UNSIGNED,
       Payment INTEGER UNSIGNED,
       primary key (Id)
    );
alter table Billing.Payments add column ForAd TINYINT(1);
alter table Billing.Payments add column AdSum NUMERIC(19,5);
alter table Billing.Payments add column Ad INTEGER UNSIGNED;
