    create table Billing.PayerAuditRecords (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       UserName VARCHAR(255),
       WriteTime DATETIME default 0  not null,
       ObjectId INTEGER UNSIGNED default 0  not null,
       ObjectType INTEGER default 0  not null,
       Name VARCHAR(255),
       Message VARCHAR(255),
       Administrator INTEGER UNSIGNED,
       Payer INTEGER UNSIGNED,
       primary key (Id),
       constraint FK_Payers_Payer FOREIGN KEY (Payer) References Payers(PayerId) on delete cascade,
       constraint FK_Admin_Administrator FOREIGN KEY (Administrator) References accessright.regionaladmins(RowId) on delete set null
    );
