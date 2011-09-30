
CREATE TABLE  `logs`.`GeneralReportLogs` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT,
  `LogTime` datetime NOT NULL,
  `OperatorName` varchar(50) NOT NULL,
  `OperatorHost` varchar(50) NOT NULL,
  `Operation` tinyint(3) unsigned NOT NULL,
  `GeneralReportCode` bigint(20) unsigned,
  `FirmCode` int(11) unsigned,
  `Allow` tinyint(1),
  `EMailSubject` varchar(255),
  `ReportFileName` varchar(255),
  `ReportArchName` varchar(255),
  `ContactGroupId` int(10) unsigned,
  `Temporary` tinyint(1),
  `TemporaryCreationDate` datetime,
  `Comment` varchar(255),
  `PayerID` int(10) unsigned,
  `Format` enum('Excel','DBF'),

  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=cp1251;

DROP TRIGGER IF EXISTS reports.GeneralReportLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER reports.GeneralReportLogDelete AFTER DELETE ON reports.general_reports
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.GeneralReportLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		GeneralReportCode = OLD.GeneralReportCode,
		FirmCode = OLD.FirmCode,
		Allow = OLD.Allow,
		EMailSubject = OLD.EMailSubject,
		ReportFileName = OLD.ReportFileName,
		ReportArchName = OLD.ReportArchName,
		ContactGroupId = OLD.ContactGroupId,
		Temporary = OLD.Temporary,
		TemporaryCreationDate = OLD.TemporaryCreationDate,
		Comment = OLD.Comment,
		PayerID = OLD.PayerID,
		Format = OLD.Format;
END;

DROP TRIGGER IF EXISTS reports.GeneralReportLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER reports.GeneralReportLogUpdate AFTER UPDATE ON reports.general_reports
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.GeneralReportLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		GeneralReportCode = OLD.GeneralReportCode,
		FirmCode = NULLIF(NEW.FirmCode, OLD.FirmCode),
		Allow = NULLIF(NEW.Allow, OLD.Allow),
		EMailSubject = NULLIF(NEW.EMailSubject, OLD.EMailSubject),
		ReportFileName = NULLIF(NEW.ReportFileName, OLD.ReportFileName),
		ReportArchName = NULLIF(NEW.ReportArchName, OLD.ReportArchName),
		ContactGroupId = NULLIF(NEW.ContactGroupId, OLD.ContactGroupId),
		Temporary = NULLIF(NEW.Temporary, OLD.Temporary),
		TemporaryCreationDate = NULLIF(NEW.TemporaryCreationDate, OLD.TemporaryCreationDate),
		Comment = NULLIF(NEW.Comment, OLD.Comment),
		PayerID = NULLIF(NEW.PayerID, OLD.PayerID),
		Format = NULLIF(NEW.Format, OLD.Format);
END;

DROP TRIGGER IF EXISTS reports.GeneralReportLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER reports.GeneralReportLogInsert AFTER INSERT ON reports.general_reports
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.GeneralReportLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		GeneralReportCode = NEW.GeneralReportCode,
		FirmCode = NEW.FirmCode,
		Allow = NEW.Allow,
		EMailSubject = NEW.EMailSubject,
		ReportFileName = NEW.ReportFileName,
		ReportArchName = NEW.ReportArchName,
		ContactGroupId = NEW.ContactGroupId,
		Temporary = NEW.Temporary,
		TemporaryCreationDate = NEW.TemporaryCreationDate,
		Comment = NEW.Comment,
		PayerID = NEW.PayerID,
		Format = NEW.Format;
END;

