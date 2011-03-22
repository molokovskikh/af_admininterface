alter table future.Users add column RootService INTEGER UNSIGNED;
alter table Future.Suppliers add column RegionMask BIGINT UNSIGNED default 0 not null;

    create table Future.AssignedServices (
        Id INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
       RegionMask BIGINT UNSIGNED default 0  not null,
       User INTEGER UNSIGNED,
       Service INTEGER UNSIGNED,
       primary key (Id)
    );

    create table Future.AssignedServicePermissions (
        AssignedServiceId INTEGER UNSIGNED not null,
       PermissionId INTEGER UNSIGNED not null
    );
