DROP TRIGGER IF EXISTS future.servicesUpdateTrigger;

DROP TRIGGER IF EXISTS future.servicesInsertTrigger;

CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER future.servicesInsertTrigger before INSERT ON future.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;

CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER future.servicesUpdateTrigger before update ON future.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;