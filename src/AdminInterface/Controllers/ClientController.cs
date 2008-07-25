using System;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using AdminInterface.Extentions;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(ADHelper)), 
		Rescue("Fail", typeof(LoginNotFoundException)),
		Rescue("Fail", typeof(CantChangePassword)),
		Secure, 
	]
	public class ClientController : SmartDispatcherController
	{
		public override void PreSendView(object view)
		{
			if (view is IControllerAware)
				((IControllerAware)view).SetController(this, ControllerContext);
			base.PreSendView(view);
		}

		public void Info(uint cc)
		{
			var client = Client.Find(cc);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			PropertyBag["Client"] = client;
			PropertyBag["Admin"] = SecurityContext.Administrator;
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
		}

		[Layout("Common")]
		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(uint ouar)
		{
			var user = User.GetById(ouar);
			var client = user.Client;

			SecurityContext.Administrator
				.CheckClientHomeRegion(client.HomeRegion.Id)
				.CheckClientType(client.Type);
			
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
									 string payReason, 
									 string freeReason)
		{
			var user = User.GetById(userId);
			var administrator = SecurityContext.Administrator;

			administrator
				.CheckClientType(user.Client.Type)
				.CheckClientHomeRegion(user.Client.HomeRegion.Id);

			user.CheckLogin();

			var password = Func.GeneratePassword();
			var userName = administrator.UserName;
			var host = Context.Request.UserHostAddress;
			var reason = isFree ? freeReason : payReason;
		
			using (new TransactionScope())
			{
				ADHelper.ChangePassword(user.Login, password);

				DbLogHelper.SavePersistentWithLogParams(userName,
				                                        host,
				                                        ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof (ClientInfoLogEntity)));

				if (user.Client.Type == ClientType.Drugstore)
					user.Client.ResetUin();

				new ClientInfoLogEntity
				{
					UserName = administrator.UserName,
					ClientCode = user.Client.Id,
					WriteTime = DateTime.Now
				}.SetProblem(isFree, reason)
				.Save();

				var passwordChangeLog = new PasswordChangeLogEntity
				{
					ClientHost = host,
					LogTime = DateTime.Now,
					TargetUserName = user.Login,
					UserName = administrator.UserName,
				};			

				if (isSendClientCard)
				{
					var smtpId = ReportHelper.SendClientCardAfterPasswordChange(user.Client,
					                                                            user,
					                                                            password,
					                                                            additionEmailsToNotify);
					passwordChangeLog.SmtpId = smtpId;
					passwordChangeLog.SetSentTo(additionEmailsToNotify, user.Client.GetAddressForSendingClientCard());
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
				RedirectToAction("SuccessPasswordChanged");
			else
			{
				PrepareSessionForReport(user, password);
				RedirectToUrl("../report.aspx");
			}
		}

		public void SuccessPasswordChanged()
		{}

		private void PrepareSessionForReport(User user, string password)
		{
			Session["AccessGrant"] = 1;
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