insert into Future.AssignedServices(User, Service)
select Id, RootService from Future.Users;
