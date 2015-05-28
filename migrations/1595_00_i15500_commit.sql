USE usersettings;
ALTER TABLE defaults ADD COLUMN NewSupplierMailText TEXT NULL; 
ALTER TABLE defaults ADD COLUMN NewSupplierMailSubject VARCHAR(255) NULL; 