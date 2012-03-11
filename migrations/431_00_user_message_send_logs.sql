
    create table Logs.UserMessageSendLogs (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       LogTime DATETIME not null,
       Message VARCHAR(255),
       User INTEGER UNSIGNED not null,
       Admin INTEGER UNSIGNED not null,
       primary key (Id),
       constraint FK_UserMessageSendLogs_User foreign key (User) references Future.Users(Id) on delete cascade,
       constraint FK_UserMessageSendLog_Admin foreign key (Admin) references Accessright.RegionalAdmins(RowId) on delete cascade
    );
