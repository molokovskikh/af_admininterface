using System;
using System.IO;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using AdminInterface.Extentions;
using Common.Tools;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(ADHelper)),
		Helper(typeof(ViewHelper)),
		Helper(typeof(HttpUtility)),
		Rescue("Fail", typeof(LoginNotFoundException)),
		Rescue("Fail", typeof(CantChangePassword)),
		Secure(PermissionType.ViewDrugstore, PermissionType.ViewDrugstore, Required = Required.AnyOf),
		Layout("General"),
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

		public void Info(uint cc)
		{
			var client = Client.FindAndCheck(cc);
			var authorizationLog = AuthorizationLogEntity.TryFind(client.Id);

			PropertyBag["authorizationLog"] = authorizationLog;
			PropertyBag["Client"] = client;
			if (String.IsNullOrEmpty(client.Registrant))
				PropertyBag["Registrant"] = null;
			else
				PropertyBag["Registrant"] = Administrator.GetByName(client.Registrant);
			PropertyBag["Admin"] = SecurityContext.Administrator;
			PropertyBag["logs"] = ClientInfoLogEntity.MessagesForClient(client);
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client)
		{
			SecurityContext.Administrator.CheckClientPermission(client);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<Client>(SecurityContext.Administrator.UserName,
				                                                     Request.UserHostAddress);
				client.Update();
			}

			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToAction("info", new { cc = client.Id });
		}

		[AccessibleThrough(Verb.Post)]
		public void SendMessage(string message, uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			using (new TransactionScope())
				new ClientInfoLogEntity(message, client.Id).Save();

			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToAction("info", new {cc = client.Id});
		}

		//[AccessibleThrough(Verb.Get)]
		public void SearchOffers(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);
			PropertyBag["client"] = client;
		}

		//[AccessibleThrough(Verb.Post)]
		public void SearchOffers(uint clientCode, string searchText)
		{
			var client = Client.FindAndCheck(clientCode);
			if (!String.IsNullOrEmpty(searchText))
			{
				var offers = Offer.Search(client, searchText);
				PropertyBag["Offers"] = offers;
			}
			PropertyBag["Client"] = client;
		}

		[RequiredPermission(PermissionType.ChangePassword)]
		public void ChangePassword(uint userId)
		{
			var user = User.GetById(userId);
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

			var password = Func.GeneratePassword();
			var userName = administrator.UserName;
			var host = Context.Request.UserHostAddress;
		
			using (new TransactionScope())
			{
				ADHelper.ChangePassword(user.Login, password);

			    DbLogHelper.SetupParametersForTriggerLogging<ClientInfoLogEntity>(userName, host);

				if (user.Client.Type == ClientType.Drugstore)
					user.Client.ResetUin();

				ClientInfoLogEntity.PasswordChange(user.Client.Id, isFree, reason).Save();

				var passwordChangeLog = new PasswordChangeLogEntity(host, user.Login, administrator.UserName);			

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

		public void Unlock(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			foreach(var user in client.GetUsers())
				if (ADHelper.IsLoginExists(user.Login) && ADHelper.IsLocked(user.Login))
					ADHelper.Unlock(user.Login);

			Flash["Message"] = Message.Notify("Разблокировано");
			RedirectToAction("Info", new { cc = client.Id });
		}

		public void DeletePreparedData(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			try
			{
				foreach (var user in client.GetUsers())
				{
					var file = String.Format(@"U:\wwwroot\ios\Results\{0}.zip", user.Id);
					if (File.Exists(file))
						File.Delete(file);
				}
				Flash["Message"] = Message.Notify("Подготовленные данные удалены");
			}
			catch
			{
				Flash["Message"] = Message.Error("Ошибка удаления подготовленных данных, попробуйте позднее.");
			}
			RedirectToAction("Info", new { cc = client.Id });
		}

		public void ResetUin(uint clientCode, string reason)
		{
			var client = Client.FindAndCheck(clientCode);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<Client>(
					new
						{
							inHost = Request.UserHostAddress,
							inUser = SecurityContext.Administrator.UserName,
							ResetIdCause = reason
						});
				ClientInfoLogEntity.ReseteUin(client.Id, reason).Save();																			
				client.ResetUin();
				RedirectToAction("Info", new { cc = client.Id });
			}
		}

		public void ShowUsersPermissions(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			PropertyBag["Client"] = client;
			PropertyBag["Permissions"] = UserPermission.FindPermissionsAvailableFor(client);
		}

		public void UpdateUsersPermissions(uint clientCode,
										   [ARDataBind("users", Expect = _hackForBinder, AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] User[] users)
		{
			
			var client = Client.FindAndCheck(clientCode);

			using (new TransactionScope())
				users.Each(u => u.Update());

			RedirectToAction("ShowUsersPermissions", new { clientCode = client.Id });
		}

		public void SuccessPasswordChanged()
		{}
	}
}