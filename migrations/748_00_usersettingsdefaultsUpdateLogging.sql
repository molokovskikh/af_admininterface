
alter table Logs.DefaultLogs
add column AllowedMiniMailExtensions varchar(255),
add column ResponseSubjectMiniMailOnUnknownProvider varchar(255),
add column ResponseBodyMiniMailOnUnknownProvider mediumtext,
add column ResponseSubjectMiniMailOnEmptyRecipients varchar(255),
add column ResponseBodyMiniMailOnEmptyRecipients mediumtext,
add column ResponseSubjectMiniMailOnMaxAttachment varchar(255),
add column ResponseBodyMiniMailOnMaxAttachment mediumtext,
add column ResponseSubjectMiniMailOnAllowedExtensions varchar(255),
add column ResponseBodyMiniMailOnAllowedExtensions mediumtext,
add column ResponseSubjectMiniMailOnEmptyLetter varchar(255),
add column ResponseBodyMiniMailOnEmptyLetter mediumtext,
add column AddressesHelpText text,
add column TechOperatingModeTemplate text,
add column TechOperatingModeBegin varchar(5),
add column TechOperatingModeEnd varchar(5)
;

DROP TRIGGER IF EXISTS usersettings.DefaultLogDelete;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER usersettings.DefaultLogDelete AFTER DELETE ON usersettings.defaults
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.DefaultLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		AnalitFVersion = OLD.AnalitFVersion,
		DefaultId = OLD.Id,
		FormaterId = OLD.FormaterId,
		SenderId = OLD.SenderId,
		EmailFooter = OLD.EmailFooter,
		Phones = OLD.Phones,
		AllowedMiniMailExtensions = OLD.AllowedMiniMailExtensions,
		ResponseSubjectMiniMailOnUnknownProvider = OLD.ResponseSubjectMiniMailOnUnknownProvider,
		ResponseBodyMiniMailOnUnknownProvider = OLD.ResponseBodyMiniMailOnUnknownProvider,
		ResponseSubjectMiniMailOnEmptyRecipients = OLD.ResponseSubjectMiniMailOnEmptyRecipients,
		ResponseBodyMiniMailOnEmptyRecipients = OLD.ResponseBodyMiniMailOnEmptyRecipients,
		ResponseSubjectMiniMailOnMaxAttachment = OLD.ResponseSubjectMiniMailOnMaxAttachment,
		ResponseBodyMiniMailOnMaxAttachment = OLD.ResponseBodyMiniMailOnMaxAttachment,
		ResponseSubjectMiniMailOnAllowedExtensions = OLD.ResponseSubjectMiniMailOnAllowedExtensions,
		ResponseBodyMiniMailOnAllowedExtensions = OLD.ResponseBodyMiniMailOnAllowedExtensions,
		ResponseSubjectMiniMailOnEmptyLetter = OLD.ResponseSubjectMiniMailOnEmptyLetter,
		ResponseBodyMiniMailOnEmptyLetter = OLD.ResponseBodyMiniMailOnEmptyLetter,
		AddressesHelpText = OLD.AddressesHelpText,
		TechOperatingModeTemplate = OLD.TechOperatingModeTemplate,
		TechOperatingModeBegin = OLD.TechOperatingModeBegin,
		TechOperatingModeEnd = OLD.TechOperatingModeEnd;
END;

DROP TRIGGER IF EXISTS usersettings.DefaultLogUpdate;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER usersettings.DefaultLogUpdate AFTER UPDATE ON usersettings.defaults
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.DefaultLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		AnalitFVersion = NULLIF(NEW.AnalitFVersion, OLD.AnalitFVersion),
		DefaultId = OLD.Id,
		FormaterId = NULLIF(NEW.FormaterId, OLD.FormaterId),
		SenderId = NULLIF(NEW.SenderId, OLD.SenderId),
		EmailFooter = NULLIF(NEW.EmailFooter, OLD.EmailFooter),
		Phones = NULLIF(NEW.Phones, OLD.Phones),
		AllowedMiniMailExtensions = NULLIF(NEW.AllowedMiniMailExtensions, OLD.AllowedMiniMailExtensions),
		ResponseSubjectMiniMailOnUnknownProvider = NULLIF(NEW.ResponseSubjectMiniMailOnUnknownProvider, OLD.ResponseSubjectMiniMailOnUnknownProvider),
		ResponseBodyMiniMailOnUnknownProvider = NULLIF(NEW.ResponseBodyMiniMailOnUnknownProvider, OLD.ResponseBodyMiniMailOnUnknownProvider),
		ResponseSubjectMiniMailOnEmptyRecipients = NULLIF(NEW.ResponseSubjectMiniMailOnEmptyRecipients, OLD.ResponseSubjectMiniMailOnEmptyRecipients),
		ResponseBodyMiniMailOnEmptyRecipients = NULLIF(NEW.ResponseBodyMiniMailOnEmptyRecipients, OLD.ResponseBodyMiniMailOnEmptyRecipients),
		ResponseSubjectMiniMailOnMaxAttachment = NULLIF(NEW.ResponseSubjectMiniMailOnMaxAttachment, OLD.ResponseSubjectMiniMailOnMaxAttachment),
		ResponseBodyMiniMailOnMaxAttachment = NULLIF(NEW.ResponseBodyMiniMailOnMaxAttachment, OLD.ResponseBodyMiniMailOnMaxAttachment),
		ResponseSubjectMiniMailOnAllowedExtensions = NULLIF(NEW.ResponseSubjectMiniMailOnAllowedExtensions, OLD.ResponseSubjectMiniMailOnAllowedExtensions),
		ResponseBodyMiniMailOnAllowedExtensions = NULLIF(NEW.ResponseBodyMiniMailOnAllowedExtensions, OLD.ResponseBodyMiniMailOnAllowedExtensions),
		ResponseSubjectMiniMailOnEmptyLetter = NULLIF(NEW.ResponseSubjectMiniMailOnEmptyLetter, OLD.ResponseSubjectMiniMailOnEmptyLetter),
		ResponseBodyMiniMailOnEmptyLetter = NULLIF(NEW.ResponseBodyMiniMailOnEmptyLetter, OLD.ResponseBodyMiniMailOnEmptyLetter),
		AddressesHelpText = NULLIF(NEW.AddressesHelpText, OLD.AddressesHelpText),
		TechOperatingModeTemplate = NULLIF(NEW.TechOperatingModeTemplate, OLD.TechOperatingModeTemplate),
		TechOperatingModeBegin = NULLIF(NEW.TechOperatingModeBegin, OLD.TechOperatingModeBegin),
		TechOperatingModeEnd = NULLIF(NEW.TechOperatingModeEnd, OLD.TechOperatingModeEnd);
END;

DROP TRIGGER IF EXISTS usersettings.DefaultLogInsert;
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER usersettings.DefaultLogInsert AFTER INSERT ON usersettings.defaults
FOR EACH ROW BEGIN
	INSERT
	INTO `logs`.DefaultLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		AnalitFVersion = NEW.AnalitFVersion,
		DefaultId = NEW.Id,
		FormaterId = NEW.FormaterId,
		SenderId = NEW.SenderId,
		EmailFooter = NEW.EmailFooter,
		Phones = NEW.Phones,
		AllowedMiniMailExtensions = NEW.AllowedMiniMailExtensions,
		ResponseSubjectMiniMailOnUnknownProvider = NEW.ResponseSubjectMiniMailOnUnknownProvider,
		ResponseBodyMiniMailOnUnknownProvider = NEW.ResponseBodyMiniMailOnUnknownProvider,
		ResponseSubjectMiniMailOnEmptyRecipients = NEW.ResponseSubjectMiniMailOnEmptyRecipients,
		ResponseBodyMiniMailOnEmptyRecipients = NEW.ResponseBodyMiniMailOnEmptyRecipients,
		ResponseSubjectMiniMailOnMaxAttachment = NEW.ResponseSubjectMiniMailOnMaxAttachment,
		ResponseBodyMiniMailOnMaxAttachment = NEW.ResponseBodyMiniMailOnMaxAttachment,
		ResponseSubjectMiniMailOnAllowedExtensions = NEW.ResponseSubjectMiniMailOnAllowedExtensions,
		ResponseBodyMiniMailOnAllowedExtensions = NEW.ResponseBodyMiniMailOnAllowedExtensions,
		ResponseSubjectMiniMailOnEmptyLetter = NEW.ResponseSubjectMiniMailOnEmptyLetter,
		ResponseBodyMiniMailOnEmptyLetter = NEW.ResponseBodyMiniMailOnEmptyLetter,
		AddressesHelpText = NEW.AddressesHelpText,
		TechOperatingModeTemplate = NEW.TechOperatingModeTemplate,
		TechOperatingModeBegin = NEW.TechOperatingModeBegin,
		TechOperatingModeEnd = NEW.TechOperatingModeEnd;
END;