DROP FUNCTION if exists Usersettings.GetClientUpdateTime;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` FUNCTION Usersettings.`GetClientUpdateTime`(inClientCode INTEGER) RETURNS datetime
    READS SQL DATA
BEGIN
  DECLARE ClientUpdateTime Datetime;
  DECLARE varPrimaryClientCode Integer;
  DECLARE varIncludeType tinyint(1);

  select
    PrimaryClientCode,
    includetype
  into
    varPrimaryClientCode,
    varIncludeType 
  from
    usersettings.ClientsData
    left join usersettings.includeregulation on includeregulation.IncludeClientCode = ClientsData.FirmCode
  where
    ClientsData.FirmCode = inClientCode 
  limit 1;

  if ((varPrimaryClientCode is not null) and (varIncludeType in (0, 3))) 
  then
    select max(UserUpdateInfo.UpdateDate)
    into ClientUpdateTime
    from
      Usersettings.OSUserAccessRight,
      Usersettings.UserUpdateInfo
    where
        OSUserAccessRight.ClientCode = varPrimaryClientCode
    and UserUpdateInfo.UserId = OSUserAccessRight.RowId;

    return ClientUpdateTime;
  else
    select max(UserUpdateInfo.UpdateDate)
    into ClientUpdateTime
    from
      Usersettings.OSUserAccessRight,
      Usersettings.UserUpdateInfo
    where
        OSUserAccessRight.ClientCode = inClientCode
    and UserUpdateInfo.UserId = OSUserAccessRight.RowId;

    return ClientUpdateTime;
  end if;
END;
