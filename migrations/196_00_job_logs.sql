
    create table logs.JobLogs (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       Name VARCHAR(255),
       ExecuteTime BIGINT default 0  not null,
       LogTime DATETIME default 0  not null,
       Message VARCHAR(255),
       primary key (Id)
    );
