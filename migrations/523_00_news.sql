
    create table Usersettings.News (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       PublicationDate DATETIME default 0  not null,
       Header VARCHAR(255),
       Body VARCHAR(255),
       Deleted TINYINT(1) default 0  not null,
       UpdateTime timestamp not null default current_timestamp on update current_timestamp,
       primary key (Id)
    );
