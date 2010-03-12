using System;
using System.IO;
using System.Linq;
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
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using AdminInterface.Properties;
using log4net;
using AdminInterface.Services;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(ADHelper)),
		Helper(typeof(ViewHelper)),
		Helper(typeof(HttpUtility)),
		Rescue("Fail", typeof(LoginNotFoundException)),
		Rescue("Fail", typeof(CantChangePassword)),
		Secure(PermissionType.ViewDrugstore, PermissionType.ViewDrugstore, Required = Required.AnyOf),
		Layout("GeneralWithJQuery"),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class ClientController : ARSmartDispatcherController
	{
		public void Info(uint cc)
		{
			var client = Client.FindAndCheck(cc);			
			PropertyBag["Client"] = client;
			if (String.IsNullOrEmpty(client.Registrant))
				PropertyBag["Registrant"] = null;
			else
				PropertyBag["Registrant"] = Administrator.GetByName(client.Registrant);
			PropertyBag["Admin"] = SecurityContext.Administrator;
			PropertyBag["logs"] = ClientInfoLogEntity.MessagesForClient(client);
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
			PropertyBag["CallLogs"] = CallLog.LastCalls();
			PropertyBag["CiUrl"] = Settings.Default.ClientInterfaceUrl;

			var users = client.GetUsers();
			PropertyBag["users"] = users.OrderBy(usr => usr.Login);

			var authorizationLogs = AuthorizationLogEntity.GetEntitiesByUsers(users.ToList());
			PropertyBag["authorizationLogs"] = new AuthorizationLogEntityList(authorizationLogs);

			try
			{
				var usersInfo = ADHelper.GetPartialUsersInformation(users);
				PropertyBag["usersInfo"] = usersInfo;
			}
			catch (Exception ex)
			{
				var userInfo = new ADUserInformationCollection();
				string usersNames = "";
				foreach (var user in users)
				{
					userInfo.Add(new ADUserInformation() {Login = user.Login});
					usersNames += user.Login + ", ";
				}
				PropertyBag["usersInfo"] = userInfo;
				
				LogManager.GetLogger(typeof(ClientController)).Error(
					String.Format("Не смогли получить информацию о пользователе AD. ClientId={0}, Admin={1}, Users=({2})",
						client.Id, SecurityContext.Administrator.UserName, usersNames));
			}

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
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void UpdateDrugstore([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client,
			ulong homeRegion,
			[ARDataBind("drugstore", AutoLoad = AutoLoadBehavior.Always)] DrugstoreSettings drugstore,
			[DataBind("regionSettings")] RegionSettings[] regionSettings)
		{
			SecurityContext.Administrator.CheckClientPermission(client);
			using (var scope = new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging<Client>(SecurityContext.Administrator.UserName,
																	 Request.UserHostAddress);
				var oldMaskRegion = client.MaskRegion;
				foreach (var setting in regionSettings)
				{
					client.HomeRegion = Region.Find(homeRegion);
					client.Update();
					if (setting.IsAvaliableForBrowse)
						client.MaskRegion |= setting.Id;
					else
						client.MaskRegion &= ~setting.Id;
					if (setting.IsAvaliableForOrder)
						drugstore.OrderRegionMask |= setting.Id;
					else
						drugstore.OrderRegionMask &= ~setting.Id;
				}				
				if (oldMaskRegion != client.MaskRegion)
					client.MaintainIntersection();
				client.Update();
				drugstore.Update();
				scope.VoteCommit();
			}
			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToUrl(String.Format("../client/{0}", client.Id));
		}

		[AccessibleThrough(Verb.Post)]
		public void BindPhone(uint clientCode, string phone)
		{
			var client = Client.FindAndCheck(clientCode);
			var group = client.ContactGroupOwner.ContactGroups.FirstOrDefault(c => c.Type == ContactGroupType.KnownPhones);
			if (group == null)
				group = client.ContactGroupOwner.AddContactGroup(ContactGroupType.KnownPhones);
			phone = phone.Substring(0, 4) + "-" + phone.Substring(4, phone.Length - 4);
			group.AddContact(new Contact{ ContactText = phone, Type = ContactType.Phone});
			using(new TransactionScope())
			{
				ArHelper.WithSession(s => s.CreateSQLQuery(@"
update logs.CallLogs
set Id2 = 0
where `from` = :phone")
					.SetParameter("phone", phone.Replace("-", ""))
					.ExecuteUpdate());
				group.Save();
			}
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void SendMessage(string message, uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			if (!String.IsNullOrEmpty(message))
			{
				using (new TransactionScope())
					new ClientInfoLogEntity(message, client.Id).Save();

				Flash["Message"] = Message.Notify("Сохранено");
			}
			RedirectToReferrer();
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

		public void Unlock(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			foreach(var user in client.GetUsers())
				if (ADHelper.IsLoginExists(user.Login) && ADHelper.IsLocked(user.Login))
					ADHelper.Unlock(user.Login);

			Flash["Message"] = Message.Notify("Разблокировано");
			RedirectToReferrer();
		}

		public void DeletePreparedData(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			try
			{
				foreach (var user in client.GetUsers())
				{
					var file = String.Format(Settings.Default.UserPreparedDataFormatString, user.Id);
					if (File.Exists(file))
						File.Delete(file);
				}
				Flash["Message"] = Message.Notify("Подготовленные данные удалены");
			}
			catch
			{
				Flash["Message"] = Message.Error("Ошибка удаления подготовленных данных, попробуйте позднее.");
			}
			RedirectToReferrer();
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
				RedirectToReferrer();
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void DrugstoreSettings(uint clientId)
		{
			var client = Client.Find(clientId);
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			var drugstore = Models.DrugstoreSettings.Find(clientId);
			PropertyBag["client"] = client;
			PropertyBag["regions"] = regions;
			PropertyBag["drugstore"] = drugstore;
		}

		public void NotifySuppliers(uint clientId)
		{
			var client = Client.Find(clientId);
			new NotificationService().NotifySupplierAboutDrugstoreRegistration(client);
			Mailer.ClientRegistrationResened(client);
			CancelView();
			CancelLayout();
			Flash["Message"] = Message.Notify("Уведомления отправлены");
			RedirectToReferrer();
		}
	}
}