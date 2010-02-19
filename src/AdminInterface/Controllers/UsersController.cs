using System;
using System.IO;
using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
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
	[Helper(typeof(HttpUtility)),
	 Layout("NewDefault")]
	public class UsersController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			PropertyBag["client"] = client;
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
			PropertyBag["emailForSend"] = client.GetAddressForSendingClientCard();
			PropertyBag["EmailContactType"] = ContactType.Email;
			PropertyBag["PhoneContactType"] = ContactType.Phone;
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("user")] User user, [DataBind("contacts")] Contact[] contacts, uint clientId, bool sendClientCard, string mails)
		{
			var client = Client.FindAndCheck(clientId);
			string password;
			PasswordChangeLogEntity passwordChangeLog;
			using(var scope = new TransactionScope(OnDispose.Rollback))
			{
				user.Client = client;
				user.Setup(client);
				password = user.CreateInAd();
				passwordChangeLog = new PasswordChangeLogEntity(user.Login);
				passwordChangeLog.Save();
				client.Addresses.Each(a => a.SetAccessControl(user.Login));
				user.UpdateContacts(contacts);
				scope.VoteCommit();
			}

			Mailer.UserRegistred(user);

			if (sendClientCard && !String.IsNullOrEmpty(mails) && !String.IsNullOrEmpty(mails.Trim()))
			{
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
			PropertyBag["permissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.Base);
			PropertyBag["ExcelPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFExcel);
			PropertyBag["PrintPermissions"] = UserPermission.FindPermissionsByType(UserPermissionTypes.AnalitFPrint);
			PropertyBag["logs"] = ClientInfoLogEntity.MessagesForUser(user);
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
		public void Update([ARDataBind("user", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "user.AssignedPermissions, user.AvaliableAddresses, user.InheritPricesFrom")] User user,
			[DataBind("WorkRegions")] ulong[] workRegions, 
			[DataBind("OrderRegions")] ulong[] orderRegions,
			[DataBind("contacts")] Contact[] contacts,
			[DataBind("deletedContacts")] Contact[] deletedContacts)
		{
			ulong temp = 0;
			workRegions.Each(r => { temp += r; });
			user.WorkRegionMask = temp;
			temp = 0;
			orderRegions.Each(r => { temp += r; });
			user.OrderRegionMask = temp;

			user.UpdateContacts(contacts, deletedContacts);
			user.Update();
			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("users", "Edit", new { login = user.Login });
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

				DbLogHelper.SetupParametersForTriggerLogging<ClientInfoLogEntity>(userName, host);

				if (user.Client.Type == ClientType.Drugstore)
					user.Client.ResetUin();

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
				RedirectToUrl("../report.aspx");
			}
		}

		public void SuccessPasswordChanged()
		{}

		private void PrepareSessionForReport(User user, string password, bool isRegistration)
		{
			Session["Register"] = false;
			Session["Code"] = user.Client.Id;
			Session["DogN"] = user.Client.BillingInstance.PayerID;
			Session["Name"] = user.Client.FullName;
			Session["ShortName"] = user.Client.Name;
			Session["Login"] = user.Login;
			Session["Password"] = password;
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
				ClientInfoLogEntity.ReseteUin(user.Client.Id, reason).Save();
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
	}
}
