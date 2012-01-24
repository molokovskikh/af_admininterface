DROP TRIGGER IF EXISTS future.servicesUpdateTrigger;

DROP TRIGGER IF EXISTS future.servicesInsertTrigger;

CREATE TRIGGER future.servicesInsertTrigger before INSERT ON future.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;

CREATE TRIGGER future.servicesUpdateTrigger before update ON future.services
FOR EACH ROW BEGIN
set new.RegionCode = NEW.HomeRegion;
end;