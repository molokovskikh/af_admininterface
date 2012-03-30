DROP PROCEDURE Reports.GetAllClientCode;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` PROCEDURE Reports.`GetAllClientCode`(in inFilter varchar(255), in inID bigint)
begin
  declare filterStr varchar(257);
  if (inID is not null) then
    select
      cl.Id as ID,
      convert(concat(cl.Id, '-', cl.Name) using cp1251) as DisplayValue
    from
       Customers.clients cl
    where
        cl.Id = inID
    order by cl.Name;
  else
    if ((inFilter is not null) and (length(inFilter) > 0)) then
      set filterStr = concat('%', inFilter, '%');
      select
        cl.Id as ID,
        convert(concat(cl.Id, '-', cl.Name) using cp1251) as DisplayValue
      from
        Customers.clients cl
      where
           cl.Name like filterStr        
      order by cl.Name;
    else
      select
        cl.Id as ID,
        convert(concat(cl.Id, '-', cl.Name) using cp1251) as DisplayValue
      from        
        Customers.clients cl
      order by cl.Name;
    end if;
  end if;
end;
