using System;
using AddUser;
using AdminInterface.Filters;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Filter(ExecuteEnum.BeforeAction, typeof(AuthorizeFilter)), Helper(typeof(ADHelper))]
	public class ClientController : SmartDispatcherController
	{
		public void Info(uint cc)
		{
			var client = Client.Find(cc);
			PropertyBag["Client"] = client;
			PropertyBag["Admin"] = Session["Admin"];
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
		}

		[Layout("Common")]
		public void ChangePassword(uint cc, uint ouar)
		{
			var client = Client.Find(cc);
			var user = User.Find(ouar);
			PropertyBag["client"] = client;
			PropertyBag["user"] = user;
		}

		public void DoPasswordChange(uint clientCode, 
									 uint userId, 
									 string additionEmailsToNotify, 
									 bool isSendClientCard, 
									 bool isFree, 
									 string payReason, 
									 string freeReason)
		{
			var client = Client.Find(clientCode);
			var user = User.Find(userId);
			var administrator = ((Administrator) Session["Admin"]);
			var password = Func.GeneratePassword();
			var userName = administrator.UserName;
			var host = Context.Request.UserHostAddress;
			var reason = isFree ? freeReason : payReason;

			ADHelper.ChangePassword(user.Login, password);
		
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SavePersistentWithLogParams(userName,
				                                        host,
				                                        ActiveRecordMediator.GetSessionFactoryHolder().CreateSession(typeof (ClientInfoLogEntity)));
				
				new ClientInfoLogEntity
				{
					UserName = userName,
					ClientCode = clientCode,
					WriteTime = DateTime.Now
				}.SetProblem(isFree, reason)
				.Save();

				new PasswordChangeLogEntity
				{
					ClientHost = Context.Request.UserHostAddress,
					LogTime = DateTime.Now,
					TargetUserName = user.Login,
					UserName = userName,
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

			PrepareSessionForReport(user, password, client);

			if (isSendClientCard)
				ReportHelper.SendClientCard(client, user, password, additionEmailsToNotify, false);

			Redirect("../report.aspx");
		}

		private void PrepareSessionForReport(User user, string password, Client client)
		{
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