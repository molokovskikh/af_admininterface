using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using AdminInterface.Properties;
using System.Web;
using Common.Web.Ui.Models;
using System.Linq;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(HttpUtility)),
		Layout("GeneralWithJQueryOnly"),
		Secure,
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class UsersController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			PropertyBag["client"] = client;
			PropertyBag["drugstore"] = client.Settings;
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
			PropertyBag["emailForSend"] = client.GetAddressForSendingClientCard();
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			PropertyBag["regions"] = regions;
			PropertyBag["Organizations"] = client.Orgs().ToArray();
			PropertyBag["UserRegistration"] = true;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("user")] User user, 
			[DataBind("contacts")] Contact[] contacts, 
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			[ARDataBind("address", AutoLoadBehavior.NewRootInstanceIfInvalidKey)] Address address,
			[DataBind("persons")] Person[] persons,
			string comment,
			bool sendClientCard,
			uint clientId,
			string mails)
		{
			var client = Client.FindAndCheck(clientId);
			string password;
			PasswordChangeLogEntity passwordChangeLog;
			if (String.IsNullOrEmpty(address.Value))
				address = null;
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();

				user.Setup(client);
				password = user.CreateInAd();
				user.WorkRegionMask = regionSettings.GetBrowseMask();
				user.OrderRegionMask = regionSettings.GetOrderMask();
				passwordChangeLog = new PasswordChangeLogEntity(user.Login);
				passwordChangeLog.Save();
				user.UpdateContacts(contacts);
				user.UpdatePersons(persons);

				if (address != null)
				{
					address = client.AddAddress(address);
					address.AvaliableForUsers = new List<User> {user};
					address.SaveAndFlush();
					address.Maintain();
				}
				client.Save();

				scope.VoteCommit();
			}

			if (address != null)
				address.CreateFtpDirectory();

			client.Addresses.Each(a => a.SetAccessControl(user.Login));

			Mailer.Registred(user, comment);
			if (address != null)
				Mailer.Registred(address, null);

			var haveMails = (!String.IsNullOrEmpty(mails) && !String.IsNullOrEmpty(mails.Trim())) ||
				(contacts.Where(contact => contact.Type == ContactType.Email).Count() > 0);
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

				Flash["Message"] = new Message("Пользователь создан");
				RedirectUsingRoute("client", "info", new {cc = client.Id});
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
			var user = User.GetById(id);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl;
			PropertyBag["user"] = user;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["client"] = user.Client;
			PropertyBag["logs"] = ClientInfoLogEntity.MessagesForUser(user);
			PropertyBag["authorizationLog"] = user.Logs;
			PropertyBag["userInfo"] = ADHelper.GetADUserInformation(user.Login);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			var setting = user.Client.Settings;
			PropertyBag["AllowWorkRegions"] = Region.GetRegionsByMask(user.Client.MaskRegion).OrderBy( reg => reg.Name);
			PropertyBag["AllowOrderRegions"] = Region.GetRegionsByMask(setting.OrderRegionMask).OrderBy(reg => reg.Name);
			if (String.IsNullOrEmpty(user.Registrant))
				PropertyBag["Registrant"] = null;
			else 
				PropertyBag["Registrant"] = Administrator.GetByName(user.Registrant);
			PropertyBag["RegistrationDate"] = user.RegistrationDate;
			if ((user.ContactGroup != null) && (user.ContactGroup.Contacts != null))
				PropertyBag["ContactGroup"] = user.ContactGroup;
			if (user.Client.Status == ClientStatus.Off || user.Enabled == false) 
				PropertyBag["enabled"] = false;
			else 
				PropertyBag["enabled"] = true;
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
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();

				user.WorkRegionMask = workRegions.Aggregate(0UL, (v, a) => a + v);
				user.OrderRegionMask = orderRegions.Aggregate(0UL, (v, a) => a + v);
				user.UpdateContacts(contacts, deletedContacts);
				user.UpdatePersons(persons, deletedPersons);
				user.Update();
				scope.VoteCommit();
			}

			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("users", "Edit", new {id = user.Id});
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(uint id)
		{
			var user = User.GetById(id);
			var client = user.Client;

			SecurityContext.Administrator.CheckClientPermission(client);
			
			user.CheckLogin();

			PropertyBag["client"] = client;
			PropertyBag["user"] = user;
			PropertyBag["emailForSend"] = client.GetAddressForSendingClientCard();
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void DoPasswordChange(uint userId, 
									 string emailsForSend, 
									 bool isSendClientCard, 
									 bool isFree, 
									 bool changeLogin,
									 string reason)
		{
			var user = User.GetById(userId);
			var administrator = SecurityContext.Administrator;
			
			administrator
				.CheckClientType(user.Client.Type)
				.CheckClientHomeRegion(user.Client.HomeRegion.Id);
			
			user.CheckLogin();
			
			var password = User.GeneratePassword();
		
			using (new TransactionScope())
			{
				ADHelper.ChangePassword(user.Login, password);
				if (changeLogin)
					ADHelper.RenameUser(user.Login, user.Id.ToString());

				DbLogHelper.SetupParametersForTriggerLogging();

				if (user.Client.Type == ClientType.Drugstore)
					user.ResetUin();
				if (changeLogin)
				{
					user.Login = user.Id.ToString();
					user.Save();
				}
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
			
			NotificationHelper.NotifyAboutPasswordChange(user.Client,
				administrator,
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

		public void Unlock(string login)
		{
#if !DEBUG
			if (ADHelper.IsLoginExists(login) && ADHelper.IsLocked(login))
				ADHelper.Unlock(login);
#endif
			Flash["Message"] = Message.Notify("Разблокировано");
			RedirectToReferrer();
		}

		public void DeletePreparedData(uint userCode)
		{			
			try
			{
				var user = User.GetById(userCode);
				var file = String.Format(CustomSettings.UserPreparedDataFormatString, user.Id);
				if (File.Exists(file))
					File.Delete(file);
				Flash["Message"] = Message.Notify("Подготовленные данные удалены");
			}
			catch
			{
				Flash["Message"] = Message.Error("Ошибка удаления подготовленных данных, попробуйте позднее.");
			}
			RedirectToReferrer();
		}

		public void ResetUin(uint userCode, string reason)
		{
			var user = User.GetById(userCode);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging(
					new
					{
						inHost = Request.UserHostAddress,
						inUser = SecurityContext.Administrator.UserName,
						ResetIdCause = reason
					});
				ClientInfoLogEntity.ReseteUin(user, reason).Save();
				user.ResetUin();
				Flash["Message"] = Message.Notify("УИН сброшен");
				RedirectToReferrer();
			}
		}

		[AccessibleThrough(Verb.Post)]
		public void SendMessage(string message, uint clientCode, uint userId)
		{
			var client = Client.FindAndCheck(clientCode);
			var user = User.Find(userId);

			if (!String.IsNullOrEmpty(message))
			{
				using (new TransactionScope())
					new ClientInfoLogEntity(message, user).Save();

				Flash["Message"] = Message.Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Get)]
		public void Settings(uint id)
		{
			var user = User.GetById(id);
			PropertyBag["user"] = user;
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
		}

		[AccessibleThrough(Verb.Post)]
		public void SaveSettings([ARDataBind("user", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "user.AssignedPermissions, user.InheritPricesFrom")] User user)
		{
			using (var scope = new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				user.Update();
				scope.VoteCommit();
			}
			Flash["Message"] = Message.Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new { id = user.Id });
		}

		public void SearchOffers(uint id, string searchText)
		{
			var user = User.Find(id);
			if (!String.IsNullOrEmpty(searchText))
			{
				PropertyBag["Offers"] = Offer.Search(user, searchText);
			}
			PropertyBag["user"] = user;
		}
	}
}
