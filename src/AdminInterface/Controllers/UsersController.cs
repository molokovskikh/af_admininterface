using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using AddUser;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
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
using Common.Web.Ui.NHibernateExtentions;

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
			if (service.IsClient())
			{
				PropertyBag["drugstore"] = ((Client)service).Settings;
				PropertyBag["Organizations"] = ((Client)service).Orgs().ToArray();
				PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			}
			else {
				PropertyBag["singleRegions"] = true;
				PropertyBag["registerSupplierUser"] = true;
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

			user.Init(service);

			string password;
			PasswordChangeLogEntity passwordChangeLog;
			if (String.IsNullOrEmpty(address.Value))
				address = null;
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();

				user.Payer = Payer.Find(user.Payer.Id);
				user.Setup();
				password = user.CreateInAd();
				user.WorkRegionMask = regionSettings.GetBrowseMask();
				user.OrderRegionMask = regionSettings.GetOrderMask();
				passwordChangeLog = new PasswordChangeLogEntity(user.Login);
				passwordChangeLog.Save();
				user.UpdateContacts(contacts);
				user.UpdatePersons(persons);

				if (service.IsClient() && address != null)
				{
					address = ((Client)service).AddAddress(address);
					user.RegistredWith(address);
					address.SaveAndFlush();
					address.Maintain();
				}
				service.Save();

				scope.VoteCommit();
			}

			if (address != null)
				address.CreateFtpDirectory();

			Mailer.Registred(user, comment);
			user.AddBillingComment(comment);
			if (address != null)
			{
				address.AddBillingComment(comment);
				Mailer.Registred(address, comment);
			}

			var haveMails = (!String.IsNullOrEmpty(mails) && !String.IsNullOrEmpty(mails.Trim())) ||
				(contacts.Where(contact => contact.Type == ContactType.Email).Any());
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
					mails);
				passwordChangeLog.SetSentTo(smtpId, mails);
				passwordChangeLog.Update();

				Notify("Пользователь создан");
				if (service.IsClient())
					RedirectUsingRoute("Clients", "show", new {service.Id});
				else
					RedirectUsingRoute("Suppliers", "show", new {service.Id});
			}
			else
			{
				Flash["password"] = password;
				Redirect("main", "report", new {id = user.Id});
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id)
		{
			var user = User.Find(id);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl;
			PropertyBag["user"] = user;
			if (user.Client != null)
				PropertyBag["client"] = user.Client;

			PropertyBag["Messages"] = ClientInfoLogEntity.MessagesForUser(user);
			PropertyBag["authorizationLog"] = user.Logs;
			PropertyBag["userInfo"] = ADHelper.GetADUserInformation(user.Login);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			PropertyBag["maxRegion"] = UInt64.MaxValue;
			if (user.Client != null)
			{
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
				Edit(user.Id);
				RenderView("Edit");
				return;
			}

			user.WorkRegionMask = workRegions.Aggregate(0UL, (v, a) => a + v);
			user.OrderRegionMask = orderRegions.Aggregate(0UL, (v, a) => a + v);
			user.UpdateContacts(contacts, deletedContacts);
			user.UpdatePersons(persons, deletedPersons);
			user.Save();

			Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new {id = user.Id});
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(uint id)
		{
			var user = User.Find(id);
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
			var user = User.Find(userId);
			user.CheckLogin();
			var administrator = Admin;
			var password = User.GeneratePassword();
		
			using (new TransactionScope())
			{
				ADHelper.ChangePassword(user.Login, password);
				if (changeLogin)
					ADHelper.RenameUser(user.Login, user.Id.ToString());

				DbLogHelper.SetupParametersForTriggerLogging();
				user.ResetUin();
				if (changeLogin)
					user.Login = user.Id.ToString();
				user.Save();
				ClientInfoLogEntity.PasswordChange(user, isFree, reason).Save();

				var passwordChangeLog = new PasswordChangeLogEntity(user.Login);

				if (isSendClientCard)
				{
					var smtpId = ReportHelper.SendClientCard(
						user,
						password,
						false,
						emailsForSend);
					passwordChangeLog.SetSentTo(smtpId, emailsForSend);
				}

				passwordChangeLog.Save();
			}
			
			NotificationHelper.NotifyAboutPasswordChange(administrator,
				user,
				password,
				isFree,
				Context.Request.UserHostAddress,
				reason);

			if (isSendClientCard)
			{
				RedirectToAction("SuccessPasswordChanged");
			}
			else
			{
				Flash["password"] = password;
				Redirect("main", "report", new {id = user.Id, isPasswordChange = true});
			}
		}

		public void SuccessPasswordChanged()
		{}

		public void Unlock(uint id)
		{
			var user = User.Find(id);
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
				var user = User.Find(id);
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
			var user = User.Find(id);
			DbLogHelper.SetupParametersForTriggerLogging(new {
				ResetIdCause = reason
			});
			ClientInfoLogEntity.ReseteUin(user, reason).Save();
			user.ResetUin();
			Notify("УИН сброшен");
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void SendMessage(string message, uint clientCode, uint userId)
		{
			var user = User.Find(userId);

			if (!String.IsNullOrEmpty(message))
			{
				new ClientInfoLogEntity(message, user).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Get)]
		public void Settings(uint id)
		{
			var user = User.Find(id);
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
			user.Save();
			Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new { id = user.Id });
		}

		public void SearchOffers(uint id, string searchText)
		{
			var user = User.Find(id);
			if (!String.IsNullOrEmpty(searchText))
				PropertyBag["Offers"] = Offer.Search(user, searchText);
			PropertyBag["user"] = user;
		}

		public void Delete(uint id)
		{
			var user = User.Find(id);

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
			return ActiveRecordLinqBase<User>
				.Queryable
				.Where(u => (u.Name.Contains(text) || u.Id == id))
				.OrderBy(u => u.Id)
				.Take(50)
				.ToArray()
				.Select(p => new {id = p.Id, name = String.Format("{0} - {1}", p.Id, p.Name)})
				.ToArray();
		}
	}
}
