using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using AdminInterface.Properties;
using System.Web;
using Common.Web.Ui.Models;
using System.Linq;
using log4net;

namespace AdminInterface.Controllers
{
	[
	Helper(typeof(HttpUtility)),
    Layout("NewDefault"),
	Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class UsersController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			PropertyBag["client"] = client;
			PropertyBag["drugstore"] = DrugstoreSettings.Find(client.Id);
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
			PropertyBag["emailForSend"] = client.GetAddressForSendingClientCard();
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			PropertyBag["regions"] = regions;
			PropertyBag["UserRegistration"] = true;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("user")] User user, 
			[DataBind("contacts")] Contact[] contacts, 
			uint clientId, 
			bool sendClientCard, 
			string mails,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			string deliveryAddress,
			[DataBind("persons")] Person[] persons)
		{
			var client = Client.FindAndCheck(clientId);
			string password;
			PasswordChangeLogEntity passwordChangeLog;
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				user.Client = client;
				user.Setup(client);
				password = user.CreateInAd();
				user.WorkRegionMask = regionSettings.GetBrowseMask();
				user.OrderRegionMask = regionSettings.GetOrderMask();
				passwordChangeLog = new PasswordChangeLogEntity(user.Login);
				passwordChangeLog.Save();
				while (true)
				{
					var index = 0;
					try
					{
						client.Addresses.Each(a => a.SetAccessControl(user.Login));
						break;
					}
					catch(Exception e)
					{
						LogManager.GetLogger(this.GetType()).Error("Ошибка при назначении прав, пробую еще раз", e);
						index++;
						Thread.Sleep(500);
						if (index > 3)
							break;
					}
				}
				user.UpdateContacts(contacts);
				user.UpdatePersons(persons);
				scope.VoteCommit();
                user.Client.UpdateBeAccounted();
                scope.VoteCommit();
			}
			Mailer.UserRegistred(user);

			if (!String.IsNullOrEmpty(deliveryAddress))
				using (var scope = new TransactionScope(OnDispose.Rollback))
				{
					DbLogHelper.SetupParametersForTriggerLogging<Address>(SecurityContext.Administrator.UserName,
						HttpContext.Current.Request.UserHostAddress);
					var address = new Address { Client = client, Enabled = true, Value = deliveryAddress };
					address.AvaliableForUsers = new List<User> { user };
					address.Save();
					address.MaitainIntersection();
					address.CreateFtpDirectory();
					client.Users.Each(u => address.SetAccessControl(u.Login));
					scope.VoteCommit();
					Mailer.DeliveryAddressRegistred(address);
					var settings = DrugstoreSettings.Find(client.Id);
					if (!settings.ServiceClient && client.BillingInstance.PayerID != 921)
						new NotificationService().NotifySupplierAboutAddressRegistration(address);
				}

			var haveMails = (!String.IsNullOrEmpty(mails) && !String.IsNullOrEmpty(mails.Trim())) ||
				(contacts.Where(contact => contact.Type == ContactType.Email).Count() > 0);
			// Если установлена галка отсылать рег. карту на email и задан email (в спец поле или в контактной информации)
			if (sendClientCard && haveMails)
			{
				var contactEmails = String.Empty;
				foreach (var contact in contacts)
					if (contact.Type == ContactType.Email)
						contactEmails = String.Concat(contactEmails, String.Format("{0},", contact.ContactText));
				mails = String.Concat(contactEmails, mails);
				if (mails.EndsWith(","))
					mails = mails.Remove(mails.Length - 1);
				var smtpId = ReportHelper.SendClientCardAfterPasswordChange(user.Client,
					user,
					password,
					mails);
				passwordChangeLog.SetSentTo(smtpId, mails);
				passwordChangeLog.Update();

				Flash["Message"] = new Message("Пользователь создан");
				RedirectUsingRoute("client", "info", new {cc = client.Id});
			}
			else
			{
				PrepareSessionForReport(user, password, true);
				RedirectToUrl("../report.aspx");
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(string login)
		{
			var user = User.GetByLogin(login);
			PropertyBag["CiUrl"] = Settings.Default.ClientInterfaceUrl;
			PropertyBag["user"] = user;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["client"] = user.Client;
			PropertyBag["logs"] = ClientInfoLogEntity.MessagesForUserAndClient(user);
			PropertyBag["authorizationLog"] = AuthorizationLogEntity.TryFind(user.Id);
			PropertyBag["userInfo"] = ADHelper.GetADUserInformation(user.Login);
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;

			var setting = DrugstoreSettings.Find(user.Client.Id);
			PropertyBag["AllowWorkRegions"] = Region.GetRegionsByMask(user.Client.MaskRegion).OrderBy( reg => reg.Name);
			PropertyBag["AllowOrderRegions"] = Region.GetRegionsByMask(setting.OrderRegionMask).OrderBy(reg => reg.Name);

			if (String.IsNullOrEmpty(user.Registrant))
				PropertyBag["Registrant"] = null;
			else
				PropertyBag["Registrant"] = Administrator.GetByName(user.Registrant);

			if ((user.ContactGroup != null) && (user.ContactGroup.Contacts != null))
				PropertyBag["ContactGroup"] = user.ContactGroup;
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("user", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "user.AvaliableAddresses")] User user,
			[DataBind("WorkRegions")] ulong[] workRegions, 
			[DataBind("OrderRegions")] ulong[] orderRegions,
			[DataBind("contacts")] Contact[] contacts,
			[DataBind("deletedContacts")] Contact[] deletedContacts,
			[DataBind("persons")] Person[] persons,
			[DataBind("deletedPersons")] Person[] deletedPersons)
		{
			using (var scope = new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				ulong temp = 0;
				workRegions.Each(r => { temp += r; });
				user.WorkRegionMask = temp;
				temp = 0;
				orderRegions.Each(r => { temp += r; });
				user.OrderRegionMask = temp;
				user.UpdateContacts(contacts, deletedContacts);
				user.UpdatePersons(persons, deletedPersons);
				user.Update();
				scope.VoteCommit();
				Flash["Message"] = new Message("Сохранено");
				RedirectUsingRoute("users", "Edit", new {login = user.Login});
			}
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(string login)
		{
			var user = User.GetByLogin(login);
			var client = user.Client;

			SecurityContext.Administrator.CheckClientPermission(client);
			
			user.CheckLogin();

			PropertyBag["client"] = client;
			PropertyBag["user"] = user;
			PropertyBag["emailForSend"] = client.GetAddressForSendingClientCard();
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void DoPasswordChange(uint userId, 
									 string additionEmailsToNotify, 
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
			var userName = administrator.UserName;
			var host = Context.Request.UserHostAddress;
		
			using (new TransactionScope())
			{
				ADHelper.ChangePassword(user.Login, password);
				if (changeLogin)
					ADHelper.RenameUser(user.Login, user.Id.ToString());

				DbLogHelper.SetupParametersForTriggerLogging<ClientInfoLogEntity>(userName, host);

				if (user.Client.Type == ClientType.Drugstore)
					user.Client.ResetUin();
				if (changeLogin)
				{
					user.Login = user.Id.ToString();
					user.Save();
				}
				ClientInfoLogEntity.PasswordChange(user, isFree, reason).Save();

				var passwordChangeLog = new PasswordChangeLogEntity(user.Login);

				if (isSendClientCard)
				{
					var smtpId = ReportHelper.SendClientCardAfterPasswordChange(user.Client,
						user,
						password,
						additionEmailsToNotify);
					passwordChangeLog.SetSentTo(smtpId, additionEmailsToNotify);
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
				PrepareSessionForReport(user, password, false);
				var virtualDir = Context.UrlInfo.AppVirtualDir;
				if (!virtualDir.StartsWith("/"))
					virtualDir = "/" + virtualDir;
				if (virtualDir.EndsWith("/"))
					virtualDir = virtualDir.Remove(virtualDir.Length - 1, 1);
				RedirectToUrl(virtualDir + "/report.aspx");
			}
		}

		public void SuccessPasswordChanged()
		{}

		private void PrepareSessionForReport(User user, string password, bool isRegistration)
		{
			Session["Register"] = false;
			Session["Code"] = user.Client.Id;
			Session["DogN"] = user.Client.BillingInstance.PayerID;
			Session["Name"] = String.IsNullOrEmpty(user.Client.FullName) ? String.Empty : user.Client.FullName;
			Session["ShortName"] = String.IsNullOrEmpty(user.Client.Name) ? String.Empty : user.Client.Name;
			Session["Login"] = String.IsNullOrEmpty(user.Login) ? String.Empty : user.Login;
			Session["Password"] = String.IsNullOrEmpty(password) ? String.Empty : password;
			Session["Tariff"] = user.Client.Type.Description();
			Session["IsRegistration"] = isRegistration;
		}

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
				DbLogHelper.SetupParametersForTriggerLogging<Client>(
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
					new ClientInfoLogEntity(message, client.Id, user.Id).Save();

				Flash["Message"] = Message.Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Get)]
		public void UserSettings(string login)
		{
			var user = User.GetByLogin(login);
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
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
				user.Update();
				scope.VoteCommit();
			}
			Flash["Message"] = Message.Notify("Сохранено");
			RedirectUsingRoute("users", "Edit", new { login = user.Login });
		}

		public void MoveUserToAnotherClient(uint userId, uint clientId, bool moveWithAddress)
		{
			CancelLayout();
			CancelView();
			var newClient = Client.Find(clientId);
			var user = User.Find(userId);
			// Если нужно перенести вместе с адресом,
			// адрес привязан только к этому пользователю и у пользователя нет других адресов,
			// тогда переносим адрес
			if (moveWithAddress && 
				(user.AvaliableAddresses.Count == 1) && 
				(user.AvaliableAddresses[0].AvaliableForUsers.Count == 1) &&
				(user.AvaliableAddresses[0].AvaliableForUsers[0].Id == userId))
			{
				user.AvaliableAddresses[0].MoveToAnotherClient(newClient);
			}
			user.MoveToAnotherClient(newClient);
			Flash["Message"] = Message.Notify("Пользователь успешно перемещен");
			RedirectUsingRoute("users", "Edit", new { login = user.Login });
		}
	}
}
