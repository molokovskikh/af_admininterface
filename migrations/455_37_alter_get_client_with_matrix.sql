DROP PROCEDURE Reports.GetClientWithMatrix;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Reports.`GetClientWithMatrix`(in inFilter varchar(255), in inID bigint)
begin
  declare filterStr varchar(257);
  if (inID is not null) then
      SELECT CL.Name as DisplayValue,
      R.ClientCode as ID
      FROM usersettings.RetClientsSet R, Customers.Clients CL
      where R.BuyingMatrixPriceId is not null and
      CL.ID = R.ClientCode and
      CL.ID = inID;
  else
    if ((inFilter is not null) and (length(inFilter) > 0)) then
      set filterStr = concat('%', inFilter, '%');
        SELECT CL.Name as DisplayValue,
        R.ClientCode as ID
        FROM usersettings.RetClientsSet R, Customers.Clients CL
        where R.BuyingMatrixPriceId is not null and
        CL.ID = R.ClientCode and
        CL.Name like filterStr;
    else
      SELECT CL.Name as DisplayValue,
      R.ClientCode as ID
      FROM usersettings.RetClientsSet R, Customers.Clients CL
      where R.BuyingMatrixPriceId is not null and
      CL.ID = R.ClientCode;
    end if;
  end if;
end;
