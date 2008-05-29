using System;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Filter(ExecuteWhen.BeforeAction, typeof(AuthorizeFilter)), Helper(typeof(ADHelper))]
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

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			PropertyBag["client"] = client;
			PropertyBag["user"] = user;
			PropertyBag["emailForSend"] = client.GetAddressForSendingClientCard();
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void DoPasswordChange(uint clientCode, 
									 uint userId, 
									 string additionEmailsToNotify, 
									 bool isSendClientCard, 
									 bool isFree, 
									 string payReason, 
									 string freeReason)
		{
			var client = Client.Find(clientCode);
			var user = User.GetById(userId);
			var administrator = SecurityContext.Administrator;
			var password = Func.GeneratePassword();
			var userName = administrator.UserName;
			var host = Context.Request.UserHostAddress;
			var reason = isFree ? freeReason : payReason;
		
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				ADHelper.ChangePassword(user.Login, password);

				DbLogHelper.SavePersistentWithLogParams(userName,
				                                        host,
				                                        ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof (ClientInfoLogEntity)));
				new ClientInfoLogEntity
				{
					UserName = administrator.UserName,
					ClientCode = client.Id,
					WriteTime = DateTime.Now
				}.SetProblem(isFree, reason)
				.Save();

				new PasswordChangeLogEntity
				{
					ClientHost = host,
					LogTime = DateTime.Now,
					TargetUserName = user.Login,
					UserName = administrator.UserName,
				}.Save();		
				
				scope.VoteCommit();
			}

			NotificationHelper.NotifyAboutPasswordChange(client,
			                                             administrator,
			                                             user,
														 password,
			                                             isFree,
			                                             Context.Request.UserHostAddress,
			                                             reason);

			if (isSendClientCard)
			{
				var smtpId = ReportHelper.SendClientCardAfterPasswordChange(client, user, password, additionEmailsToNotify);
				new ClientCardSendLogEntity(client.GetAddressForSendingClientCard(), additionEmailsToNotify, smtpId, administrator.UserName).Save();
				RedirectToAction("SuccessPasswordChanged");
			}
			else
			{
				PrepareSessionForReport(user, password, client);
				RedirectToUrl("../report.aspx");
			}
		}

		public void SuccessPasswordChanged()
		{}

		private void PrepareSessionForReport(User user, string password, Client client)
		{
			Session["AccessGrant"] = 1;
			Session["Register"] = false;
			Session["Code"] = client.Id;
			Session["DogN"] = client.BillingInstance.PayerID;
			Session["Name"] = client.FullName;
			Session["ShortName"] = client.ShortName;
			Session["Login"] = user.Login;
			Session["Password"] = password;
			Session["Tariff"] = BindingHelper.GetDescription(client.Type);
		}
	}
}