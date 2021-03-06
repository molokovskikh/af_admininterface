DROP PROCEDURE Reports.GetClientCodeWithNewUsers;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Reports.`GetClientCodeWithNewUsers`(in inFilter varchar(255), in inID bigint)
begin
  declare filterStr varchar(257);
  if (inID is not null) then
      select
      cl.Id,
      cl.Name ShortName,
      convert(concat(cl.ID, '-', cl.Name) using cp1251) as DisplayValue
    from
      Customers.Clients cl
    where
          cl.Id = inID
      and cl.Status = 1
    order by ShortName;
  else
    if ((inFilter is not null) and (length(inFilter) > 0)) then
      set filterStr = concat('%', inFilter, '%');     
       select
        cl.Id,
        cl.Name ShortName,
        convert(concat(cl.Id, '-', cl.Name) using cp1251) as DisplayValue
      from
        Customers.Clients cl
      where
           cl.Name like filterStr
        and cl.Status = 1
      order by ShortName;
    else
       select
        cl.Id,
        cl.Name ShortName,
        convert(concat(cl.Id, '-', cl.Name) using cp1251) as DisplayValue
      from
        Customers.Clients cl
      where
            cl.Status = 1
      order by ShortName;
    end if;
  end if;
end;
