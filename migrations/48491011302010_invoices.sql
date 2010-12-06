
    create table billing.Invoices (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Sum NUMERIC(19,5),
       Date DATETIME,
       Period INTEGER,
       Recipient INTEGER UNSIGNED,
       Payer INTEGER UNSIGNED,
       primary key (Id)
    );
alter table Billing.Payments add column RecipientId INTEGER UNSIGNED;
