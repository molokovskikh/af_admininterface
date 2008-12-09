using System;
using System.IO;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
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
	public class ClientController : ARSmartDispatcherController
	{
		//Скорбная песнь:
		/*
		 * ARбиндер в монорельсе, делает следующиее:
		 * сначала получает объект из базы затем проходит по всему набору полей и заполняет тем что было в запросе
		 * но тут возникает засада: коллекции которых нет в запросе это может значить что их вообще не нужно бигдить или то 
		 * что они должны стать пустые, что бы решить эту проблему есть свойство Expect все коллекции которые в нем перечислены будут
		 * обнулены перед биндингом, но тут есть новая засада если исходный объект это коллекция то мы в беде 
		 * потому что нужно указывать индекс для каждого элемента коллекции от сюда и хак
		*/
		private const string _hackForBinder = "users.0.AssignedPermissions, users.1.AssignedPermissions, users.2.AssignedPermissions, users.3.AssignedPermissions, users.4.AssignedPermissions, users.5.AssignedPermissions, users.6.AssignedPermissions, users.7.AssignedPermissions, users.8.AssignedPermissions, users.9.AssignedPermissions, users.10.AssignedPermissions";
		public override void PreSendView(object view)
		{
			if (view is IControllerAware)
				((IControllerAware)view).SetController(this, ControllerContext);
			base.PreSendView(view);
		}

		public void Info(uint cc)
		{
			var client = Client.Find(cc);
			var authorizationLog = AuthorizationLogEntity.TryFind(client.Id);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);

			PropertyBag["authorizationLog"] = authorizationLog;
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

			    DbLogHelper.SetupParametersForTriggerLogging<ClientInfoLogEntity>(userName, host);

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

		public void Unlock(uint clientCode)
		{
			var client = Client.Find(clientCode);
			SecurityContext.Administrator.CheckClientPermission(client);
			foreach(var user in client.GetUsers())
				if (ADHelper.IsLoginExists(user.Login) && ADHelper.IsLocked(user.Login))
					ADHelper.Unlock(user.Login);

			Flash["UnlockMessage"] = "Разблокировано";
			RedirectToAction("Info", new { cc = client.Id });
		}

		public void DeletePreparedData(uint clientCode)
		{
			var client = Client.Find(clientCode);
			SecurityContext.Administrator.CheckClientPermission(client);

			try
			{
				File.Delete(String.Format(@"U:\wwwroot\ios\Results\{0}.zip", client.Id));
				Flash["DeleteMessage"] = "Подготовленные данные удалены";
			}
			catch
			{
				Flash["DeleteFailMessage"] = "Ошибка удаления подготовленных данных, попробуйте позднее.";
			}
			RedirectToAction("Info", new { cc = client.Id });
		}

		public void ResetUin(uint clientCode, string reason)
		{
			var client = Client.Find(clientCode);
			SecurityContext.Administrator.CheckClientPermission(client);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<Client>(
					new
						{
							inHost = Request.UserHostAddress,
							inUser = SecurityContext.Administrator.UserName,
							ResetIdCause = reason
						});
				new ClientInfoLogEntity
					{
						UserName = SecurityContext.Administrator.UserName,
						WriteTime = DateTime.Now,
						ClientCode = client.Id,
						Message = String.Format("$$$Изменение УИН: " + reason),
					}.Save();
																			
				client.ResetUin();

				RedirectToAction("Info", new { cc = client.Id });
			}
		}

		[Layout("Common")]
		public void ShowUsersPermissions(uint clientCode)
		{
			var client = Client.Find(clientCode);

			SecurityContext.Administrator.CheckClientPermission(client);

			PropertyBag["Client"] = client;
			PropertyBag["Permissions"] = UserPermission.FindPermissionsAvailableFor(client);
		}

		public void UpdateUsersPermissions(uint ClientCode,
										   [ARDataBind("users", Expect = _hackForBinder, AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] User[] users)
		{
			
			var client = Client.Find(ClientCode);

			SecurityContext.Administrator.CheckClientPermission(client);

			using (new TransactionScope())
			{
				foreach (var user in users)
					user.UpdateAndFlush();
			}
			RedirectToAction("ShowUsersPermissions", new { clientCode = client.Id });
		}
	}
}