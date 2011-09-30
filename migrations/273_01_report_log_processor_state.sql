
    create table Billing.ReportLogProcessorStates (
        Id INTEGER NOT NULL AUTO_INCREMENT,
       LastRun DATETIME default 0  not null,
       primary key (Id)
    );
