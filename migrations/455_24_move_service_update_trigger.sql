CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Customers.servicesUpdateTrigger before update ON Customers.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;
