drop trigger Future.servicesUpdateTrigger;
CREATE DEFINER=`RootDBMS`@`127.0.0.1` TRIGGER future.servicesUpdateTrigger before update ON future.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;
