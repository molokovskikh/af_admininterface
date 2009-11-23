using AdminInterface.Extentions;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[Layout("NewDefault")]
	public class UsersController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			PropertyBag["client"] = client;
			PropertyBag["permissions"] = UserPermission.FindPermissionsAvailableFor(client);
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("user")] User user, uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			user.Client = client;
			user.Save();
			ADHelper.CreateUserInAD(user.Login,
				User.GeneratePassword(),
				user.Client.Id.ToString());

			Flash["Message"] = new Message("Пользователь создан");
			RedirectUsingRoute("client", "info", new { cc = client.Id });
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(string login)
		{
			var user = User.GetByLogin(login);
			PropertyBag["user"] = user;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["client"] = user.Client;
			PropertyBag["permissions"] = UserPermission.FindPermissionsAvailableFor(user.Client);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("user", AutoLoad = AutoLoadBehavior.Always, Expect = "user.AssignedPermissions, user.AvaliableAddresses")] User user)
		{
			user.Update();
			Flash["Message"] = new Message("Сохранен");
			RedirectUsingRoute("client", "info", new { cc = user.Client.Id });
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

				var passwordChangeLog = new PasswordChangeLogEntity(host, administrator.UserName, user.Login);

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
				PrepareSessionForReport(user, password);
				RedirectToUrl("../report.aspx");
			}
		}

		private void PrepareSessionForReport(User user, string password)
		{
			Session["Register"] = false;
			Session["Code"] = user.Client.Id;
			Session["DogN"] = user.Client.BillingInstance.PayerID;
			Session["Name"] = user.Client.FullName;
			Session["ShortName"] = user.Client.ShortName;
			Session["Login"] = user.Login;
			Session["Password"] = password;
			Session["Tariff"] = user.Client.Type.Description();
		}
	}
}
