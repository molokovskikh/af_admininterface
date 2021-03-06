﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Models.Telephony;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.Components.Binder;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using System.Web;
using Common.Web.Ui.Models;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using AdminInterface.Components;
using AdminInterface.Models.AFNet;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(HttpUtility)),
		Secure,
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class UsersController : AdminInterfaceController
	{
		public void Show(uint id)
		{
			RedirectUsingRoute("Edit", new { id });
		}

		[AccessibleThrough(Verb.Post)]
		public void Add()
		{
			var encode = Encoding.GetEncoding("utf-8");
			Request.InputStream.Seek(0, SeekOrigin.Begin);
			var responseStream = new StreamReader(Request.InputStream, encode);
			var json = PatchJson(responseStream.ReadLine());
			var deserializedObj = JsonConvert.DeserializeObject<User>(json);
			Add(new Contact[0],
				null,
				new Person[0],
				"Пользователь создан из интерфейса поставщика",
				true,
				deserializedObj.RootService.Id,
				string.Empty,
				json,
				null,
				null);
		}

		///<summary>
		/// Установка прав на FTP доступ. Вызывается из Клиенсткого интерфейса.
		/// </summary>
		[AccessibleThrough(Verb.Post)]
		public void SetFtpAccess()
		{
			var encode = Encoding.GetEncoding("utf-8");
			Request.InputStream.Seek(0, SeekOrigin.Begin);
			var responseStream = new StreamReader(Request.InputStream, encode);
			string jsonString = responseStream.ReadLine();
			dynamic jsonUser = JObject.Parse(jsonString);
			uint id = jsonUser.Id;
			bool ftpAccess = jsonUser.FtpAccess;
			User user = DbSession.Load<User>(id);
			user.SetFtpAccess(ftpAccess);
			CancelView();
		}

		public static string PatchJson(string request)
		{
			var json = JObject.Parse(request);
			json.Remove("Payer");
			json.Remove("Client");
			json.Remove("AvaliableAddresses");
			json.Remove("InheritPricesFrom");
			foreach (var child in json.GetValue("AssignedPermissions").Children()) {
				((JObject)child).Remove("Name");
				((JObject)child).Remove("Shortcut");
				((JObject)child).Remove("AvailableFor");
				((JObject)child).Remove("Type");
				((JObject)child).Remove("AssignDefaultValue");
				((JObject)child).Remove("OrderIndex");
			}
			return json.ToString();
		}

		/// <summary>
		/// Добавление ftp-пользователя по Id клиента (для стороннего приложения)
		/// </summary>
		/// <param name="id">Id клиента</param>
		public void AddClient(uint id)
		{
			Person[] persons = new Person[0];
			string comment = "Пользователь создан из интерфейса поставщика";
			/*Грязный ХАК, почему-то если принудительно не загрузить так, не делается Service.FindAndCheck<Service>(id)*/
			var currentClient = DbSession.Get<Client>(id);
			DbSession.Get<Supplier>(id);
			var service = Service.FindAndCheck<Service>(id);
			var user = new User(service);

			service.AddUser(user);
			user.Setup(DbSession);
			var password = user.CreateInAd(Session);
			user.SetFtpAccess(user.FtpAccess);
			var passwordChangeLog = new PasswordChangeLogEntity(user.Login);
			DbSession.Save(passwordChangeLog);
			user.UpdatePersons(persons);
			DbSession.Save(service);

			new Mailer(DbSession).Registred(user, comment, Defaults);
			user.AddBillingComment(comment);

			if (user.Client != null) {
				var message = string.Format("$$$Пользователю {0} - ({1}) подключены следующие адреса доставки: \r\n {2}",
					user.Id,
					user.Name,
					user.AvaliableAddresses.Implode(a => string.Format("\r\n {0} - ({1})", a.Id, a.Name)));
				DbSession.Save(new AuditRecord(message, user.Client) { MessageType = LogMessageType.System });
			}
			user.UseFtpGateway = true;
#if !DEBUG
			//создаем папку
			var root = ConfigurationManager.AppSettings["FtpUserFolder"]+ user.Login;
			var username = String.Format(@"ANALIT\{0}", user.Login);
			Directory.CreateDirectory(root);

			//раздаем права на папку
			var rootDirectorySecurity = Directory.GetAccessControl(root);
			var rule =
				new FileSystemAccessRule(username,
					FileSystemRights.Read,
					InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
					PropagationFlags.InheritOnly,
					AccessControlType.Allow);
			rootDirectorySecurity.AddAccessRule(rule);

			rule = new FileSystemAccessRule(username,
				FileSystemRights.Write,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);
			rootDirectorySecurity.AddAccessRule(rule);

			rule = new FileSystemAccessRule(username,
				FileSystemRights.Delete,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);
			rootDirectorySecurity.AddAccessRule(rule);

			rule = new FileSystemAccessRule(username,
				FileSystemRights.ExecuteFile,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);
			rootDirectorySecurity.AddAccessRule(rule);

			rule = new FileSystemAccessRule(username,
				FileSystemRights.ListDirectory,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);
			rootDirectorySecurity.AddAccessRule(rule);

			rule = new FileSystemAccessRule(username,
				FileSystemRights.CreateDirectories,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Allow);
			rootDirectorySecurity.AddAccessRule(rule);

			rule = new FileSystemAccessRule(username,
				FileSystemRights.WriteAttributes,
				InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
				PropagationFlags.InheritOnly,
				AccessControlType.Deny);
			rootDirectorySecurity.AddAccessRule(rule);
			Directory.SetAccessControl(root, rootDirectorySecurity);
#endif

			DbSession.Save(currentClient);
			DbSession.Save(user);
			Response.StatusCode = 200;
			RenderText(user.Login + "," + password.Password);
			CancelView();
		}

		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId, User client = null)
		{
			/*Грязный ХАК, почему-то если принудительно не загрузить так, не делается Service.FindAndCheck<Service>(clientId)*/
			DbSession.Get<Client>(clientId);
			DbSession.Get<Supplier>(clientId);

			var service = client == null ? Service.FindAndCheck<Service>(clientId) : client.RootService;
			var user = client ?? new User(service);
			PropertyBag["UserMessage"] = "Текст сообщения";
			PropertyBag["deliveryAddress"] = "";
			PropertyBag["account"] = user.Accounting;

			if (client == null) {
				PropertyBag["UseDefPermession"] = true;
				PropertyBag["SendToEmail"] = true;
				//Для вьюшки - по умолчанию должен быть True.
				//Но в модели мы это не можем разместить, так как по здравому смыслу он по-умолчанию false.
				//Так мы избежим лишних действий по созданию прав на директории
				user.FtpAccess = true;
				var rejectWaibillParams = new RejectWaibillParams().Get(clientId, DbSession);
				user.SendWaybills = rejectWaibillParams.SendWaybills;
				user.SendRejects = rejectWaibillParams.SendRejects;
			}

			PropertyBag["phonesForSendToUserList"] = user.GetPhonesForSendingSms(); //"9031848398";
			PropertyBag["phonesForSendToAdminList"] = GetAdminByRegionForSms(user.RootService.HomeRegion.Id);

			PropertyBag["client"] = service;
			if (service.IsClient()) {
				PropertyBag["drugstore"] = ((Client)service).Settings;
				var organizations = ((Client)service).Orgs().ToArray();
				if (organizations.Length == 1) {
					PropertyBag["address"] = new Address { LegalEntity = organizations.First() };
				}
				PropertyBag["Organizations"] = organizations;
				PropertyBag["permissions"] = UserPermission.FindPermissionsForDrugstore(DbSession);
			}
			else {
				PropertyBag["singleRegions"] = true;
				PropertyBag["registerSupplierUser"] = true;
				PropertyBag["availibleRegions"] = ((Supplier)service).RegionMask;
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.SupplierInterface);
			}
			PropertyBag["user"] = user;
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.AnalitFExcel);
			PropertyBag["AccessPermissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.AnalitFPrint)
				.Where(r => r.Shortcut == "ORDR" || r.Shortcut == "CASH" || r.Shortcut == "STCK").ToArray();
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.AnalitFPrint)
				.Where(r => r.Shortcut != "ORDR" && r.Shortcut != "CASH" && r.Shortcut != "STCK").ToArray();
			PropertyBag["emailForSend"] = user.GetAddressForSendingClientCard();
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			PropertyBag["regions"] = Region.All(DbSession).ToArray();
			IList<Payer> payers = new List<Payer>();
			if (service.IsClient())
				payers = ((Client)service).Payers;
			else
				payers = new List<Payer> { DbSession.Load<Supplier>(service.Id).Payer };

			if (payers.Count == 1) {
				user.Payer = payers.First();
				PropertyBag["onePayer"] = true;
			}
			else {
				PropertyBag["onePayer"] = false;
			}
			PropertyBag["Payers"] = payers;
			PropertyBag["maxRegion"] = UInt64.MaxValue;
			PropertyBag["UserRegistration"] = true;
			PropertyBag["defaultSettings"] = Defaults;
		}


		public NameValueCollection GetCollectionFromJson(string text, string objName)
		{
			var result = new NameValueCollection();
			JObject jsonObject = null;
			if (text.Contains("{"))
				jsonObject = JObject.Parse(text);
			else {
				result.Add(objName, text);
				return result;
			}
			foreach (var obj in jsonObject) {
				if (obj.Value.Type == JTokenType.Array && obj.Value.HasValues) {
					var arrayValues = (JArray)obj.Value;
					var count = 0;
					foreach (var arrayValue in arrayValues) {
						result.Add(GetCollectionFromJson(arrayValue.ToString(), string.Format("{0}.{1}[{2}]", objName, obj.Key, count)));
						count++;
					}
					continue;
				}
				if (obj.Value.Type == JTokenType.Object) {
					result.Add(GetCollectionFromJson(obj.Value.ToString(), objName + "." + obj.Key));
					continue;
				}
				var value = obj.Value.ToString();
				if (obj.Value.Type == JTokenType.Null)
					value = string.Empty;
				result.Add(objName + "." + obj.Key, value);
			}
			return result; //0
		}

		public void BindObjectInstanceForUser(User instance, string prefix, string jsonSource)
		{
			var treeRoot = new CompositeNode("root");
			if (jsonSource != null) {
				var builder = new TreeBuilder();
				var collection = GetCollectionFromJson(jsonSource, "User");
				treeRoot = builder.BuildSourceNode(collection);
			}
			else {
				treeRoot = Request.ObtainParamsNode(ParamStore.Params);
			}
			Binder.BindObjectInstance(instance, prefix, treeRoot);
			boundInstances[instance] = Binder.ErrorList;
			PopulateValidatorErrorSummary(instance, Binder.GetValidationSummary(instance));
		}

		[AccessibleThrough(Verb.Post)]
		public void Add(
			[DataBind("contacts")] Contact[] contacts,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[DataBind("persons")] Person[] persons,
			string comment,
			bool sendClientCard,
			uint clientId,
			string mails,
			string jsonSource,
			string[] phonesForSendToUserArray,
			string[] phonesForSendToAdminArray)
		{
			/*Грязный ХАК, почему-то если принудительно не загрузить так, не делается Service.FindAndCheck<Service>(clientId)*/
			DbSession.Get<Client>(clientId);
			DbSession.Get<Supplier>(clientId);
			var service = Service.FindAndCheck<Service>(clientId);
			var user = new User(service);
			var address = new Address();

			SetARDataBinder(AutoLoadBehavior.NullIfInvalidKey);
			var account = user.Accounting;

			BindObjectInstanceForUser(user, "user", jsonSource);
			BindObjectInstance(address, "address", AutoLoadBehavior.NewInstanceIfInvalidKey);
			BindObjectInstance(account, "account", AutoLoadBehavior.NewInstanceIfInvalidKey);

			if (!IsValid(user)) {
				Add(clientId, user);
				PropertyBag["account"] = account;
				PropertyBag["UserMessage"] = comment;
				PropertyBag["SendToEmail"] = sendClientCard;
				PropertyBag["emailForSend"] = mails;
				PropertyBag["InputPersonsList"] = persons;
				PropertyBag["InputContactsList"] = contacts;
				PropertyBag["SelectedRegions"] = regionSettings;
				PropertyBag["deliveryAddress"] = address.Value ?? "";
				PropertyBag["phonesForSendToUserList"] = user.GetPhonesForSendingSms();
				PropertyBag["phonesForSendToAdminList"] = GetAdminByRegionForSms(user.RootService.HomeRegion.Id);
				if (!String.IsNullOrEmpty(jsonSource)) {
					var errorSummary = Validator.GetErrorSummary(user);
					throw new Exception(errorSummary.InvalidProperties.Implode(x => $"{x} {errorSummary.GetErrorsForProperty(x).Implode()}"));
				}
				return;
			}

			if (String.IsNullOrEmpty(address.Value))
				address = null;

			if (service.IsClient() && ((Client)service).Payers.Count > 1) {
				if ((user.AvaliableAddresses.Any() && user.AvaliableAddresses.Select(s => s.LegalEntity).All(l => l.Payer.Id != user.Payer.Id)) || (address != null && address.LegalEntity.Payer.Id != user.Payer.Id)) {
					Add(service.Id);
					PropertyBag["user"] = user;
					PropertyBag["address"] = address;
					Error("Ошибка регистрации: попытка зарегистрировать пользователя и адрес в различных Плательщиках");
					return;
				}
			}

			service.AddUser(user);
			user.Setup(DbSession);
			var password = user.CreateInAd(Session);
			if (string.IsNullOrEmpty(jsonSource)) {
				user.WorkRegionMask = regionSettings.GetBrowseMask();
				user.OrderRegionMask = regionSettings.GetOrderMask();
			}
			else {
				mails = user.EmailForCard;
			}
			user.SetFtpAccess(user.FtpAccess);

			var passwordChangeLog = new PasswordChangeLogEntity(user.Login);
			DbSession.Save(passwordChangeLog);
			user.UpdateContacts(contacts);
			user.UpdatePersons(persons);

			if (service.IsClient() && address != null) {
				address = ((Client)service).AddAddress(address);
				user.RegistredWith(address);
				address.SaveAndFlush();
				address.Maintain(DbSession);
			}
			DbSession.Save(service);

			if (address != null)
				address.CreateFtpDirectory();

			new Mailer(DbSession).Registred(user, comment, Defaults);
			user.AddBillingComment(comment);
			if (address != null) {
				address.AddBillingComment(comment);
				new Mailer(DbSession).Registred(address, comment, Defaults);
			}
			if (user.Client != null) {
				var message = string.Format("$$$Пользователю {0} - ({1}) подключены следующие адреса доставки: \r\n {2}",
					user.Id,
					user.Name,
					user.AvaliableAddresses.Implode(a => string.Format("\r\n {0} - ({1})", a.Id, a.Name)));
				DbSession.Save(new AuditRecord(message, user.Client) { MessageType = LogMessageType.System });
			}

			string smsLog = ReportHelper.SendSmsPasswordToUser(user, password.Password, phonesForSendToUserArray);
			smsLog = smsLog + " " + ReportHelper.SendSmsToRegionalAdmin(user, password.Password, phonesForSendToAdminArray);
			passwordChangeLog.SmsLog = smsLog;

			var haveMails = !String.IsNullOrEmpty(mails) && !String.IsNullOrEmpty(mails.Trim());
			// Если установлена галка отсылать рег. карту на email и задан email (в спец поле или в контактной информации)
			if (sendClientCard && (haveMails || !string.IsNullOrEmpty(user.EmailForCard))) {
				var smtpId = ReportHelper.SendClientCard(user,
					password.Password,
					false,
					Defaults,
					mails);
				passwordChangeLog.SetSentTo(smtpId, new[] { mails }.Where(s => !String.IsNullOrWhiteSpace(s)).Implode());
				DbSession.Save(passwordChangeLog);

				Notify("Пользователь создан");

				if (string.IsNullOrEmpty(jsonSource)) {
					if (service.IsClient())
						RedirectUsingRoute("Clients", "show", new { service.Id });
					else
						RedirectUsingRoute("Suppliers", "show", new { service.Id });
				}
				else {
					Response.StatusCode = 200;
					CancelView();
				}
			}
			else if (string.IsNullOrEmpty(jsonSource)) {
				Redirect("main", "report", new { id = user.Id, passwordId = password.PasswordId });
			}
			else {
				Response.StatusCode = 200;
				CancelView();
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id, [SmartBinder(Expect = "filter.Types")] MessageQuery filter)
		{
			var user = DbSession.Load<User>(id);
			NHibernateUtil.Initialize(user.RootService);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl + "auth/logon.aspx";
			PropertyBag["user"] = user;
			if (user.Client != null)
				PropertyBag["client"] = user.Client;

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["authorizationLog"] = user.Logs;
			PropertyBag["userInfo"] = ADHelper.GetADUserInformation(user);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;

			PropertyBag["filter"] = filter;
			PropertyBag["messages"] = user.GetAuditRecord(DbSession, filter);

			if (user.ContactGroup != null && user.ContactGroup.Contacts != null)
				PropertyBag["ContactGroup"] = user.ContactGroup;

			if (user.RootService.Disabled || user.Enabled == false)
				PropertyBag["enabled"] = false;
			else
				PropertyBag["enabled"] = true;

			Sort.Make(this);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update(
			[ARDataBind("user", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "user.AvaliableAddresses")] User user,
			[DataBind("contacts")] Contact[] contacts,
			[DataBind("deletedContacts")] Contact[] deletedContacts,
			[DataBind("persons")] Person[] persons,
			[DataBind("deletedPersons")] Person[] deletedPersons)
		{
			if (!IsValid(user)) {
				Edit(user.Id, new MessageQuery());
				RenderView("Edit");
				return;
			}

			user.UpdateContacts(contacts, deletedContacts);
			user.UpdatePersons(persons, deletedPersons);
			DbSession.Save(user);

			Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new { id = user.Id });
		}


		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(uint id)
		{
			var user = DbSession.Load<User>(id);
			user.CheckLogin();

			PropertyBag["user"] = user;
			PropertyBag["emailForSend"] = user.GetAddressForSendingClientCard();
			PropertyBag["phonesForSendToUserList"] = user.GetPhonesForSendingSms();
			PropertyBag["phonesForSendToAdminList"] = GetAdminByRegionForSms(user.RootService.HomeRegion.Id);
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void DoPasswordChange(uint userId,
			string emailsForSend,
			bool isSendClientCard,
			bool isFree,
			bool changeLogin,
			string reason,
			string[] phonesForSendToUserArray,
			string[] phonesForSendToAdminArray)
		{
			var user = DbSession.Load<User>(userId);
			user.CheckLogin();

			var password = user.ChangePassword(Session);
			if (changeLogin) {
				ADHelper.RenameUser(user.Login, user.Id.ToString());
				user.Login = user.Id.ToString();
			}
			user.ResetUin();

			var passwordChangeLog = new PasswordChangeLogEntity(user.Login);

			if (isSendClientCard) {
				var smtpId = ReportHelper.SendClientCard(
					user,
					password.Password,
					false,
					Defaults,
					emailsForSend);
				passwordChangeLog.SetSentTo(smtpId, emailsForSend);
			}

			string smsLog = ReportHelper.SendSmsPasswordToUser(user, password.Password, phonesForSendToUserArray);
			smsLog = smsLog + " " + ReportHelper.SendSmsToRegionalAdmin(user, password.Password, phonesForSendToAdminArray);
			passwordChangeLog.SmsLog = smsLog;

			DbSession.Save(user);
			DbSession.Save(AuditRecord.PasswordChange(user, isFree, reason));
			DbSession.Save(passwordChangeLog);

			NotificationHelper.NotifyAboutPasswordChange(Admin,
				user,
				password.Password,
				isFree,
				Context.Request.UserHostAddress,
				reason);

			if (isSendClientCard) {
				Notify("Пароль успешно изменен.");
				RedirectTo(user, "Edit");
			}
			else {
				Redirect("main", "report", new { id = user.Id, isPasswordChange = true, passwordId = password.PasswordId });
			}
		}

		public void Unlock(uint id)
		{
			var user = DbSession.Load<User>(id);
			var login = user.Login;
			if (ADHelper.IsLoginExists(login) && ADHelper.IsLocked(login))
				ADHelper.Unlock(login);

			Notify("Разблокировано");
			RedirectToReferrer();
		}

		public void DeletePreparedData(uint id)
		{
			try {
				var user = DbSession.Load<User>(id);
				var files = Directory.GetFiles(Global.Config.UserPreparedDataDirectory)
					.Where(f => Regex.IsMatch(Path.GetFileName(f), string.Format(@"^({0}_)\d+?\.zip", user.Id))).ToList();
				foreach (var file in files) {
					File.Delete(file);
				}
				Notify("Подготовленные данные удалены");
			}
			catch {
				Error("Ошибка удаления подготовленных данных, попробуйте позднее.");
			}
			RedirectToReferrer();
		}

		public void ResetUin(uint id, string reason)
		{
			var user = DbSession.Load<User>(id);
			DbLogHelper.SetupParametersForTriggerLogging(new {
				ResetIdCause = reason
			});
			AuditRecord.ReseteUin(user, reason).Save();
			user.ResetUin();
			Notify("УИН сброшен");
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void SendMessage(string message, uint clientCode, uint userId)
		{
			var user = DbSession.Load<User>(userId);

			if (!String.IsNullOrEmpty(message)) {
				new AuditRecord(message, user).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Get)]
		public void Settings(uint id)
		{
			var user = DbSession.Load<User>(id);
			PropertyBag["user"] = user;
			PropertyBag["maxRegion"] = UInt64.MaxValue;
			if (user.Client == null) {
				var supplier = DbSession.Load<Supplier>(user.RootService.Id);
				if (supplier != null) {
					PropertyBag["AllowWorkRegions"] = Region.GetRegionsByMask(DbSession, supplier.RegionMask);
				}
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.SupplierInterface);
				RenderView("SupplierSettings");
			}
			else {
				var setting = user.Client.Settings;
				PropertyBag["AllowOrderRegions"] = Region.GetRegionsByMask(DbSession, setting.OrderRegionMask);
				PropertyBag["AllowWorkRegions"] = Region.GetRegionsByMask(DbSession, user.Client.MaskRegion);
				PropertyBag["permissions"] = UserPermission.FindPermissionsForDrugstore(DbSession);
				PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.AnalitFExcel);
				PropertyBag["AccessPermissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.AnalitFPrint)
					.Where(r => r.Shortcut == "ORDR" || r.Shortcut == "CASH" || r.Shortcut == "STCK").ToArray();
				PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(DbSession, UserPermissionTypes.AnalitFPrint)
					.Where(r => r.Shortcut != "ORDR" && r.Shortcut != "CASH" && r.Shortcut != "STCK").ToArray();
				if (user.AFNetConfig != null)
					user.AFNetConfig.Channels = BinChannel.Load(DbSession, user);
				RenderView("DrugstoreSettings");
			}
		}

		[AccessibleThrough(Verb.Post)]
		public uint ResetAFVersion(uint userId)
		{
			CancelView();
			var user = DbSession.Load<User>(userId);
			if (user.Client != null) {
				user.UserUpdateInfo.AFAppVersion = 999;
				DbSession.Save(user);
				Notify("Версия АФ сброшена");
			}
			else {
				Error("Нельзя сбросить версию АФ для пользователя поставщика");
			}
			return userId;
		}

		[AccessibleThrough(Verb.Post)]
		public void SaveSettings(
			[ARDataBind("user", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "user.AssignedPermissions, user.InheritPricesFrom, user.ShowUsers")] User user,
			[DataBind("WorkRegions")] ulong[] workRegions,
			[DataBind("OrderRegions")] ulong[] orderRegions)
		{
			var oldFirstTable = DbSession.OldValue(user, u => u.SubmitOrders) && DbSession.OldValue(user, u => u.IgnoreCheckMinOrder);
			if (oldFirstTable != user.FirstTable) {
				if (user.FirstTable) {
					user.Accounting.Payment = 0;
					user.Accounting.BeAccounted = false;
				}
				else {
					user.Accounting.Payment = user.Client.HomeRegion.UserPayment;
					user.Accounting.BeAccounted = false;
				}
			}
			user.WorkRegionMask = workRegions.Aggregate(0UL, (v, a) => a + v);
			if (user.Client != null)
				user.OrderRegionMask = orderRegions.Aggregate(0UL, (v, a) => a + v);

			user.ShowUsers = user.ShowUsers.Where(u => u != null).ToList();
			user.PrepareSave(DbSession);
			DbSession.Save(user);
			Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new { id = user.Id });
		}

		public void SearchOffers(uint id, string searchText)
		{
			var user = DbSession.Load<User>(id);
			if (!String.IsNullOrEmpty(searchText))
				PropertyBag["Offers"] = Offer.Search(DbSession, user, searchText);
			PropertyBag["user"] = user;
		}

		public void Delete(uint id)
		{
			var user = DbSession.Load<User>(id);

			if (user.CanDelete(DbSession)) {
				var payer = user.Payer;
				DbSession.Delete(user);
				payer.UpdatePaymentSum();
				Notify("Удалено");
				RedirectTo(user.RootService);
			}
			else {
				Error("Не могу удалить пользователя т.к. у него есть заказы");
				RedirectToReferrer();
			}
		}

		[return: JSONReturnBinder]
		public object[] SearchForShowUser(string text)
		{
			uint id;
			UInt32.TryParse(text, out id);
			var result = DbSession.Query<User>()
				.Where(u => !(!u.Login.Contains(text) && !u.Name.Contains(text) && u.Id != id && (u.RootService == null || !u.RootService.Name.Contains(text))))
				.OrderBy(u => u.Id)
				.Take(50)
				.Fetch(u => u.RootService)
				.Fetch(u => u.Logs)
				.ToList();

			var returned = new List<object>();
			foreach (var user in result) {
				if (user.Login.ToLower().Contains(text.ToLower()))
					returned.Add(new { id = user.Id, name = $"Код: {user.Id} - login: {user.Login}" });
				if (user.Name.ToLower().Contains(text.ToLower()))
					returned.Add(new { id = user.Id, name = $"Код: {user.Id} - комментарий: {user.Name}" });
				if (user.RootService != null && user.RootService.Name.ToLower().Contains(text.ToLower()))
					returned.Add(new {
						id = user.Id, name = $"Код: {user.Id} - клиент: {user.RootService.Id} - {user.RootService.Name}"
					});
			}
			return returned.ToArray();
		}

		private IList<Administrator> GetAdminByRegionForSms(ulong regionMask)
		{
			// только сотрудникам подразделений "Управление" и " Отдел регионального развития"
			var list = DbSession.Query<Administrator>().
				Where(x => (x.RegionMask & regionMask) > 0
				           && (x.Department == Department.Administration || x.Department == Department.Manager)
				           && x.PhoneSupport.StartsWith("9"))
				.OrderBy(x => x.ManagerName)
				.ToList();
			return list.Where(x => !ADHelper.IsDisabled(x.UserName)).Distinct(new AdministratorComparer()).ToList();
		}
	}
}