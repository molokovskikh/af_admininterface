﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using AddUser;
using AdminInterface.Extentions;
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
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using AdminInterface.Properties;
using System.Web;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using System.Linq;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Linq;

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
			RedirectUsingRoute("Edit", new {id});
		}

		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			/*Грязный ХАК, почему-то если принудительно не загрузить так, не делается Service.FindAndCheck<Service>(clientId)*/
			DbSession.Get<Client>(clientId);
			DbSession.Get<Supplier>(clientId);

			var service = Service.FindAndCheck<Service>(clientId);
			var user = new User(service);
			var rejectWaibillParams = new RejectWaibillParams().Get(clientId, DbSession);
			user.SendWaybills = rejectWaibillParams.SendWaybills;
			user.SendRejects = rejectWaibillParams.SendRejects;
			PropertyBag["client"] = service;
			if (service.IsClient()) {
				PropertyBag["drugstore"] = ((Client)service).Settings;
				PropertyBag["Organizations"] = ((Client)service).Orgs().ToArray();
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			}
			else {
				PropertyBag["singleRegions"] = true;
				PropertyBag["registerSupplierUser"] = true;
				PropertyBag["availibleRegions"] = ((Supplier)service).RegionMask;
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.SupplierInterface);
			}
			PropertyBag["user"] = user;
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
			PropertyBag["emailForSend"] = user.GetAddressForSendingClientCard();
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			PropertyBag["regions"] = Region.All().ToArray();
			IList<Payer> payers = new List<Payer>();
			if (service.IsClient())
				payers = ((Client)service).Payers;
			else
				payers = new List<Payer> { Supplier.Find(service.Id).Payer };
			PropertyBag["Payers"] = payers;
			PropertyBag["maxRegion"] = UInt64.MaxValue;
			PropertyBag["UserRegistration"] = true;
			PropertyBag["defaultSettings"] = Defaults;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add(
			[DataBind("contacts")] Contact[] contacts, 
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[DataBind("persons")] Person[] persons,
			string comment,
			bool sendClientCard,
			uint clientId,
			string mails)
		{
			/*Грязный ХАК, почему-то если принудительно не загрузить так, не делается Service.FindAndCheck<Service>(clientId)*/
			DbSession.Get<Client>(clientId);
			DbSession.Get<Supplier>(clientId);

			var service = Service.FindAndCheck<Service>(clientId);
			var user = new User(service);
			BindObjectInstance(user, "user");

			if (!IsValid(user)) {
				Add(service.Id);
				PropertyBag["user"] = user;
				return;
			}

			var address = new Address();
			SetBinder(new ARDataBinder());
			BindObjectInstance(address, "address", AutoLoadBehavior.NewInstanceIfInvalidKey);

			service.AddUser(user);

			if (String.IsNullOrEmpty(address.Value))
				address = null;

			user.Payer = Payer.Find(user.Payer.Id);
			user.Setup();
			var password = user.CreateInAd();
			user.WorkRegionMask = regionSettings.GetBrowseMask();
			user.OrderRegionMask = regionSettings.GetOrderMask();
			var passwordChangeLog = new PasswordChangeLogEntity(user.Login);
			DbSession.Save(passwordChangeLog);
			user.UpdateContacts(contacts);
			user.UpdatePersons(persons);

			if (service.IsClient() && address != null)
			{
				address = ((Client)service).AddAddress(address);
				user.RegistredWith(address);
				address.SaveAndFlush();
				address.Maintain();
			}
			DbSession.SaveOrUpdate(service);


			if (address != null)
				address.CreateFtpDirectory();

			Mailer.Registred(user, comment, Defaults);
			user.AddBillingComment(comment);
			if (address != null)
			{
				address.AddBillingComment(comment);
				Mailer.Registred(address, comment, Defaults);
			}
			if (user.Client != null) { 
				var message = string.Format("$$$Пользовалелю {0} - ({1}) подключены слудующие адреса доставки: \r\n {2}",
					user.Id,
					user.Name, 
					user.AvaliableAddresses.Select(a => Address.TryFind(a.Id))
						.Where(a => a != null)
						.Implode(a => string.Format("\r\n {0} - ({1})", a.Id, a.Name)));
				new AuditRecord(message, user.Client){ MessageType = LogMessageType.System }.Save();
			}

			var haveMails = (!String.IsNullOrEmpty(mails) && !String.IsNullOrEmpty(mails.Trim())) ||
				(contacts.Any(contact => contact.Type == ContactType.Email));
			// Если установлена галка отсылать рег. карту на email и задан email (в спец поле или в контактной информации)
			if (sendClientCard && haveMails)
			{
				var contactEmails = contacts
					.Where(c => c.Type == ContactType.Email)
					.Implode(c => c.ContactText);
				mails = String.Concat(contactEmails, ",", mails);
				if (mails.EndsWith(","))
					mails = mails.Remove(mails.Length - 1);
				if (mails.StartsWith(","))
					mails = mails.Substring(1, mails.Length - 1);
				var smtpId = ReportHelper.SendClientCard(user,
					password,
					false,
					Defaults,
					mails);
				passwordChangeLog.SetSentTo(smtpId, mails);
				DbSession.SaveOrUpdate(passwordChangeLog);

				Notify("Пользователь создан");
				if (service.IsClient())
					RedirectUsingRoute("Clients", "show", new {service.Id});
				else
					RedirectUsingRoute("Suppliers", "show", new {service.Id});
			}
			else
			{
				Flash["newUser"] = true;
				Flash["password"] = password;
				Redirect("main", "report", new {id = user.Id});
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id, [SmartBinder(Expect = "filter.Types")] MessageQuery filter)
		{
			var user = DbSession.Load<User>(id);
			NHibernateUtil.Initialize(user.RootService);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl;
			PropertyBag["user"] = user;
			if (user.Client != null)
				PropertyBag["client"] = user.Client;

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["authorizationLog"] = user.Logs;
			PropertyBag["userInfo"] = ADHelper.GetADUserInformation(user);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			PropertyBag["maxRegion"] = UInt64.MaxValue;

			PropertyBag["filter"] = filter;
			PropertyBag["messages"] = filter.Execute(user, DbSession);

			if (user.Client != null) {
				var setting = user.Client.Settings;
				PropertyBag["AllowOrderRegions"] = Region.GetRegionsByMask(setting.OrderRegionMask);
				PropertyBag["AllowWorkRegions"] = Region.GetRegionsByMask(user.Client.MaskRegion);
			}
			if (user.SupplierUser()) {
				var supplier = Supplier.Find(user.RootService.Id);
				if (supplier != null) {
					PropertyBag["AllowWorkRegions"] = Region.GetRegionsByMask(supplier.RegionMask);
				}
			}
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
			[DataBind("WorkRegions")] ulong[] workRegions, 
			[DataBind("OrderRegions")] ulong[] orderRegions,
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

			user.WorkRegionMask = workRegions.Aggregate(0UL, (v, a) => a + v);
			user.OrderRegionMask = orderRegions.Aggregate(0UL, (v, a) => a + v);
			user.UpdateContacts(contacts, deletedContacts);
			user.UpdatePersons(persons, deletedPersons);
			DbSession.Save(user);

			Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new {id = user.Id});
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(uint id)
		{
			var user = DbSession.Load<User>(id);
			user.CheckLogin();

			PropertyBag["user"] = user;
			PropertyBag["emailForSend"] = user.GetAddressForSendingClientCard();
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void DoPasswordChange(uint userId, 
									 string emailsForSend, 
									 bool isSendClientCard, 
									 bool isFree, 
									 bool changeLogin,
									 string reason)
		{
			var user = DbSession.Load<User>(userId);
			user.CheckLogin();
			var administrator = Admin;
			var password = User.GeneratePassword();
		
			ADHelper.ChangePassword(user.Login, password);
			if (changeLogin)
				ADHelper.RenameUser(user.Login, user.Id.ToString());

			user.ResetUin();
			if (changeLogin)
				user.Login = user.Id.ToString();
			DbSession.Save(user);
			AuditRecord.PasswordChange(user, isFree, reason).Save();

			var passwordChangeLog = new PasswordChangeLogEntity(user.Login);

			if (isSendClientCard) {
				var smtpId = ReportHelper.SendClientCard(
					user,
					password,
					false,
					Defaults,
					emailsForSend);
				passwordChangeLog.SetSentTo(smtpId, emailsForSend);
			}

			DbSession.Save(passwordChangeLog);
			NotificationHelper.NotifyAboutPasswordChange(administrator,
				user,
				password,
				isFree,
				Context.Request.UserHostAddress,
				reason);

			if (isSendClientCard) {
				Notify("Пароль успешно изменен.");
				RedirectTo(user, "Edit");
			}
			else {
				Flash["password"] = password;
				Redirect("main", "report", new {id = user.Id, isPasswordChange = true});
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
			try
			{
				var user = DbSession.Load<User>(id);
				var files = Directory.GetFiles(Global.Config.UserPreparedDataDirectory)
				.Where(f => Regex.IsMatch(Path.GetFileName(f), string.Format(@"^({0}_)\d+?\.zip", user.Id))).ToList();
				foreach (var file in files) {
					File.Delete(file);
				}
				Notify("Подготовленные данные удалены");
			}
			catch
			{
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

			if (!String.IsNullOrEmpty(message))
			{
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
			if (user.Client == null)
			{
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.SupplierInterface);
				RenderView("SupplierSettings");
			}
			else
			{
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
				PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
				PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
				RenderView("DrugstoreSettings");
			}
		}

		[AccessibleThrough(Verb.Post)]
		public void SaveSettings([ARDataBind("user", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "user.AssignedPermissions, user.InheritPricesFrom, user.ShowUsers")] User user)
		{
			DbSession.Save(user);
			Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new { id = user.Id });
		}

		public void SearchOffers(uint id, string searchText)
		{
			var user = DbSession.Load<User>(id);
			if (!String.IsNullOrEmpty(searchText))
				PropertyBag["Offers"] = Offer.Search(user, searchText);
			PropertyBag["user"] = user;
		}

		public void Delete(uint id)
		{
			var user = DbSession.Load<User>(id);

			if (user.CanDelete()) {
				var payer = user.Payer;
				user.Delete();
				payer.UpdatePaymentSum();
				Notify("Удалено");
				RedirectTo(user.RootService);
			}
			else {
				Error("Не могу удалить пользователя т.к. у него есть заказы");
				RedirectToReferrer();
			}
		}

		[return : JSONReturnBinder]
		public object[] SearchForShowUser(string text)
		{
			uint id;
			UInt32.TryParse(text, out id);
			return DbSession.Query<User>()
				.Where(u => (u.Name.Contains(text) || u.Id == id))
				.OrderBy(u => u.Id)
				.Take(50)
				.ToArray()
				.Select(p => new {id = p.Id, name = String.Format("{0} - {1}", p.Id, p.Name)})
				.ToArray();
		}
	}
}
