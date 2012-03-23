DROP PROCEDURE Usersettings.UpdatePasswords;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`UpdatePasswords`(in UpdateClientCode integer unsigned)
BEGIN
   update retclientsset set
     BaseCostPassword = GeneratePassword(),
     CodesPassword = GeneratePassword(),
     SynonymPassword = GeneratePassword()
   where
     ClientCode = UpdateClientCode;
   update intersection
   set
     LastSent = default,
     UncommittedLastSent = default
   where
     ClientCode = UpdateClientCode;
END;
