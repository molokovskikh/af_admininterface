DROP PROCEDURE if exists Usersettings.SetAFReplicationParamenter;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Usersettings.`SetAFReplicationParamenter`(IN ClientCodeParam INT UNSIGNED, IN PriceCodeParam INT UNSIGNED)
BEGIN

UPDATE AnalitFReplicationInfo,
       PricesData            ,
       OsUserAccessRight
SET    ForceReplication=1
WHERE  AnalitFReplicationInfo.UserId        =OsUserAccessRight.RowId
   AND PricesData.PriceCode     =PriceCodeParam
   AND PricesData.FirmCode      =AnalitFReplicationInfo.FirmCode
   AND OsUserAccessRight.ClientCode    =ClientCodeParam;


END;
