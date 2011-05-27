
alter table Logs.PayerLogs
add column BeginBalance decimal(10,0),
add column BeginBalanceDate datetime,
add column DocumentsOnLastWorkingDay tinyint(1),
add column DoNotGroupParts tinyint(1)
;

DROP TRIGGER IF EXISTS Billing.PayerLogDelete; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.PayerLogDelete AFTER DELETE ON Billing.Payers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PayerLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 2,
		PayerID = OLD.PayerID,
		ShortName = OLD.ShortName,
		JuridicalName = OLD.JuridicalName,
		JuridicalAddress = OLD.JuridicalAddress,
		BeforeNamePrefix = OLD.BeforeNamePrefix,
		AfterNamePrefix = OLD.AfterNamePrefix,
		INN = OLD.INN,
		KPP = OLD.KPP,
		OKPO = OLD.OKPO,
		OKVED = OLD.OKVED,
		RS = OLD.RS,
		BankName = OLD.BankName,
		KS = OLD.KS,
		BIK = OLD.BIK,
		OGRN = OLD.OGRN,
		ActualAddressCountry = OLD.ActualAddressCountry,
		ActualAddressIndex = OLD.ActualAddressIndex,
		ActualAddressProvince = OLD.ActualAddressProvince,
		ActualAddressTown = OLD.ActualAddressTown,
		ActualAddressStreet = OLD.ActualAddressStreet,
		ActualAddressHouse = OLD.ActualAddressHouse,
		ActualAddressOffice = OLD.ActualAddressOffice,
		ReceiverAddress = OLD.ReceiverAddress,
		OldPayDate = OLD.OldPayDate,
		OldTariff = OLD.OldTariff,
		DetailInvoice = OLD.DetailInvoice,
		AutoInvoice = OLD.AutoInvoice,
		PayCycle = OLD.PayCycle,
		Balance = OLD.Balance,
		DiscountPercent = OLD.DiscountPercent,
		Comment = OLD.Comment,
		ContactGroupOwnerId = OLD.ContactGroupOwnerId,
		AutoLock = OLD.AutoLock,
		NotificationType = OLD.NotificationType,
		ActualAddressRegion = OLD.ActualAddressRegion,
		HaveContract = OLD.HaveContract,
		SendRegisteredLetter = OLD.SendRegisteredLetter,
		SendScannedDocuments = OLD.SendScannedDocuments,
		DiscountValue = OLD.DiscountValue,
		DiscountType = OLD.DiscountType,
		ChangeServiceNameTo = OLD.ChangeServiceNameTo,
		ShowDiscount = OLD.ShowDiscount,
		RecipientId = OLD.RecipientId,
		EmailInvoice = OLD.EmailInvoice,
		PrintInvoice = OLD.PrintInvoice,
		SendPaymentNotification = OLD.SendPaymentNotification,
		BeginBalance = OLD.BeginBalance,
		BeginBalanceDate = OLD.BeginBalanceDate,
		DocumentsOnLastWorkingDay = OLD.DocumentsOnLastWorkingDay,
		DoNotGroupParts = OLD.DoNotGroupParts;
END;

DROP TRIGGER IF EXISTS Billing.PayerLogUpdate; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.PayerLogUpdate AFTER UPDATE ON Billing.Payers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PayerLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 1,
		PayerID = OLD.PayerID,
		ShortName = NULLIF(NEW.ShortName, OLD.ShortName),
		JuridicalName = NULLIF(NEW.JuridicalName, OLD.JuridicalName),
		JuridicalAddress = NULLIF(NEW.JuridicalAddress, OLD.JuridicalAddress),
		BeforeNamePrefix = NULLIF(NEW.BeforeNamePrefix, OLD.BeforeNamePrefix),
		AfterNamePrefix = NULLIF(NEW.AfterNamePrefix, OLD.AfterNamePrefix),
		INN = NULLIF(NEW.INN, OLD.INN),
		KPP = NULLIF(NEW.KPP, OLD.KPP),
		OKPO = NULLIF(NEW.OKPO, OLD.OKPO),
		OKVED = NULLIF(NEW.OKVED, OLD.OKVED),
		RS = NULLIF(NEW.RS, OLD.RS),
		BankName = NULLIF(NEW.BankName, OLD.BankName),
		KS = NULLIF(NEW.KS, OLD.KS),
		BIK = NULLIF(NEW.BIK, OLD.BIK),
		OGRN = NULLIF(NEW.OGRN, OLD.OGRN),
		ActualAddressCountry = NULLIF(NEW.ActualAddressCountry, OLD.ActualAddressCountry),
		ActualAddressIndex = NULLIF(NEW.ActualAddressIndex, OLD.ActualAddressIndex),
		ActualAddressProvince = NULLIF(NEW.ActualAddressProvince, OLD.ActualAddressProvince),
		ActualAddressTown = NULLIF(NEW.ActualAddressTown, OLD.ActualAddressTown),
		ActualAddressStreet = NULLIF(NEW.ActualAddressStreet, OLD.ActualAddressStreet),
		ActualAddressHouse = NULLIF(NEW.ActualAddressHouse, OLD.ActualAddressHouse),
		ActualAddressOffice = NULLIF(NEW.ActualAddressOffice, OLD.ActualAddressOffice),
		ReceiverAddress = NULLIF(NEW.ReceiverAddress, OLD.ReceiverAddress),
		OldPayDate = NULLIF(NEW.OldPayDate, OLD.OldPayDate),
		OldTariff = NULLIF(NEW.OldTariff, OLD.OldTariff),
		DetailInvoice = NULLIF(NEW.DetailInvoice, OLD.DetailInvoice),
		AutoInvoice = NULLIF(NEW.AutoInvoice, OLD.AutoInvoice),
		PayCycle = NULLIF(NEW.PayCycle, OLD.PayCycle),
		Balance = NULLIF(NEW.Balance, OLD.Balance),
		DiscountPercent = NULLIF(NEW.DiscountPercent, OLD.DiscountPercent),
		Comment = NULLIF(NEW.Comment, OLD.Comment),
		ContactGroupOwnerId = NULLIF(NEW.ContactGroupOwnerId, OLD.ContactGroupOwnerId),
		AutoLock = NULLIF(NEW.AutoLock, OLD.AutoLock),
		NotificationType = NULLIF(NEW.NotificationType, OLD.NotificationType),
		ActualAddressRegion = NULLIF(NEW.ActualAddressRegion, OLD.ActualAddressRegion),
		HaveContract = NULLIF(NEW.HaveContract, OLD.HaveContract),
		SendRegisteredLetter = NULLIF(NEW.SendRegisteredLetter, OLD.SendRegisteredLetter),
		SendScannedDocuments = NULLIF(NEW.SendScannedDocuments, OLD.SendScannedDocuments),
		DiscountValue = NULLIF(NEW.DiscountValue, OLD.DiscountValue),
		DiscountType = NULLIF(NEW.DiscountType, OLD.DiscountType),
		ChangeServiceNameTo = NULLIF(NEW.ChangeServiceNameTo, OLD.ChangeServiceNameTo),
		ShowDiscount = NULLIF(NEW.ShowDiscount, OLD.ShowDiscount),
		RecipientId = NULLIF(NEW.RecipientId, OLD.RecipientId),
		EmailInvoice = NULLIF(NEW.EmailInvoice, OLD.EmailInvoice),
		PrintInvoice = NULLIF(NEW.PrintInvoice, OLD.PrintInvoice),
		SendPaymentNotification = NULLIF(NEW.SendPaymentNotification, OLD.SendPaymentNotification),
		BeginBalance = NULLIF(NEW.BeginBalance, OLD.BeginBalance),
		BeginBalanceDate = NULLIF(NEW.BeginBalanceDate, OLD.BeginBalanceDate),
		DocumentsOnLastWorkingDay = NULLIF(NEW.DocumentsOnLastWorkingDay, OLD.DocumentsOnLastWorkingDay),
		DoNotGroupParts = NULLIF(NEW.DoNotGroupParts, OLD.DoNotGroupParts);
END;

DROP TRIGGER IF EXISTS Billing.PayerLogInsert; 
CREATE DEFINER = RootDBMS@127.0.0.1 TRIGGER Billing.PayerLogInsert AFTER INSERT ON Billing.Payers
FOR EACH ROW BEGIN
	INSERT 
	INTO `logs`.PayerLogs
	SET LogTime = now(),
		OperatorName = IFNULL(@INUser, SUBSTRING_INDEX(USER(),'@',1)),
		OperatorHost = IFNULL(@INHost, SUBSTRING_INDEX(USER(),'@',-1)),
		Operation = 0,
		PayerID = NEW.PayerID,
		ShortName = NEW.ShortName,
		JuridicalName = NEW.JuridicalName,
		JuridicalAddress = NEW.JuridicalAddress,
		BeforeNamePrefix = NEW.BeforeNamePrefix,
		AfterNamePrefix = NEW.AfterNamePrefix,
		INN = NEW.INN,
		KPP = NEW.KPP,
		OKPO = NEW.OKPO,
		OKVED = NEW.OKVED,
		RS = NEW.RS,
		BankName = NEW.BankName,
		KS = NEW.KS,
		BIK = NEW.BIK,
		OGRN = NEW.OGRN,
		ActualAddressCountry = NEW.ActualAddressCountry,
		ActualAddressIndex = NEW.ActualAddressIndex,
		ActualAddressProvince = NEW.ActualAddressProvince,
		ActualAddressTown = NEW.ActualAddressTown,
		ActualAddressStreet = NEW.ActualAddressStreet,
		ActualAddressHouse = NEW.ActualAddressHouse,
		ActualAddressOffice = NEW.ActualAddressOffice,
		ReceiverAddress = NEW.ReceiverAddress,
		OldPayDate = NEW.OldPayDate,
		OldTariff = NEW.OldTariff,
		DetailInvoice = NEW.DetailInvoice,
		AutoInvoice = NEW.AutoInvoice,
		PayCycle = NEW.PayCycle,
		Balance = NEW.Balance,
		DiscountPercent = NEW.DiscountPercent,
		Comment = NEW.Comment,
		ContactGroupOwnerId = NEW.ContactGroupOwnerId,
		AutoLock = NEW.AutoLock,
		NotificationType = NEW.NotificationType,
		ActualAddressRegion = NEW.ActualAddressRegion,
		HaveContract = NEW.HaveContract,
		SendRegisteredLetter = NEW.SendRegisteredLetter,
		SendScannedDocuments = NEW.SendScannedDocuments,
		DiscountValue = NEW.DiscountValue,
		DiscountType = NEW.DiscountType,
		ChangeServiceNameTo = NEW.ChangeServiceNameTo,
		ShowDiscount = NEW.ShowDiscount,
		RecipientId = NEW.RecipientId,
		EmailInvoice = NEW.EmailInvoice,
		PrintInvoice = NEW.PrintInvoice,
		SendPaymentNotification = NEW.SendPaymentNotification,
		BeginBalance = NEW.BeginBalance,
		BeginBalanceDate = NEW.BeginBalanceDate,
		DocumentsOnLastWorkingDay = NEW.DocumentsOnLastWorkingDay,
		DoNotGroupParts = NEW.DoNotGroupParts;
END;

