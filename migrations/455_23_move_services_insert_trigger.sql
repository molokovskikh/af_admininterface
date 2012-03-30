CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER Customers.servicesInsertTrigger before INSERT ON Customers.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;
