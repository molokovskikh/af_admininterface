using System;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Telephony;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using AdminInterface.Properties;
using log4net;

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
			PropertyBag["sortMessagesColumnName"] = "WriteTime";
			PropertyBag["sortMessagesDirection"] = "Descending";

			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["CiUrl"] = Settings.Default.ClientInterfaceUrl;

			var users = client.GetUsers();
			PropertyBag["users"] = users.OrderBy(user => user.Id);
			var authorizationLogs = AuthorizationLogEntity.GetEntitiesByUsers(users.ToList());
			PropertyBag["authorizationLogs"] = new AuthorizationLogEntityList(authorizationLogs);
			PropertyBag["sortColumnIndex"] = 0;
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

		public void Info(uint cc, int sortColumnIndex, string[] headerNames)
		{
			Info(cc);
			var client = Client.FindAndCheck(cc);
			var users = client.GetUsers();
			PropertyBag["users"] = users.SortBy(headerNames[Math.Abs(sortColumnIndex) - 1], sortColumnIndex > 0);
			PropertyBag["sortColumnIndex"] = sortColumnIndex;
		}

		public void OrderMessagesBy(string columnName, string sortDirection, uint clientId)
		{
			Info(clientId);
			var client = Client.FindAndCheck(clientId);
			PropertyBag["logs"] = ClientInfoLogEntity.MessagesForClient(client).OrderBy(columnName, sortDirection.Equals("Descending"));
			PropertyBag["sortMessagesColumnName"] = columnName;
			PropertyBag["sortMessagesDirection"] = sortDirection;
			RenderView("Info");
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client)
		{
			SecurityContext.Administrator.CheckClientPermission(client);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				client.Update();
			}

			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void UpdateDrugstore([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client,
			ulong homeRegion,
			[ARDataBind("drugstore", AutoLoad = AutoLoadBehavior.Always)] DrugstoreSettings drugstore,
			bool costsIsNoised,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			bool ActivateBuyMatrix)
		{
			SecurityContext.Administrator.CheckClientPermission(client);
			using (var scope = new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var oldMaskRegion = client.MaskRegion;
				client.HomeRegion = Region.Find(homeRegion);
				client.Update();
				client.UpdateRegionSettings(regionSettings);
				if (!costsIsNoised)
					drugstore.FirmCodeOnly = null;
				client.UpdateAndFlush();
				drugstore.UpdateAndFlush();
				if (oldMaskRegion != client.MaskRegion)
					client.MaintainIntersection();

				if (!ActivateBuyMatrix)
					drugstore.BuyingMatrixPriceId = null;
				scope.VoteCommit();
			}
			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToUrl(String.Format("../client/{0}", client.Id));
		}

		public void SuppliersForCostNoising(uint clientId)
		{
			CancelLayout();
			var client = Client.FindAndCheck(clientId);
			var suppliers = Supplier.GetByPayerId(client.BillingInstance.PayerID);
			PropertyBag["suppliers"] = suppliers;
			if (client.Settings.FirmCodeOnly.HasValue)
				PropertyBag["FirmCodeOnly"] = client.Settings.FirmCodeOnly;
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
			using(var scope = new TransactionScope())
			{
					ArHelper.WithSession(s => s.CreateSQLQuery(@"
delete from telephony.UnresolvedPhone
where Phone like :phone")
							.SetParameter("phone", phone.Replace("-", ""))
							.ExecuteUpdate());
				group.Save();
				scope.VoteCommit();
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
				DbLogHelper.SetupParametersForTriggerLogging(
					new {
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
			var drugstore = client.Settings;
			PropertyBag["client"] = client;
			PropertyBag["regions"] = regions;
			PropertyBag["drugstore"] = drugstore;
			if (drugstore.BuyingMatrixPriceId != null)
				PropertyBag["BuyMatrixPrice"] = Price.Find(drugstore.BuyingMatrixPriceId.Value);
		}

		public void NotifySuppliers(uint clientId)
		{
			var client = Client.Find(clientId);
			Mailer.ClientRegistred(client, true);
			Flash["Message"] = Message.Notify("Уведомления отправлены");
			RedirectToReferrer();
		}

		public void SearchClient(string searchText)
		{
			CancelView();
			CancelLayout();
			int searchNumber;
			Int32.TryParse(searchText, out searchNumber);
			PropertyBag["clients"] = Client.Queryable.Where(c => c.Name.Contains(searchText) || c.Id == searchNumber).OrderBy(c => c.Name).ToList();
			RenderView("SearchClientSubview");
		}

		[AccessibleThrough(Verb.Get)]
		[return: JSONReturnBinder]
		public object[] LegalEntities(uint id)
		{
			var client = Client.FindAndCheck(id);
			return client.Payer.JuridicalOrganizations.Select(j => new {j.Id, j.Name}).ToArray();
		}

		public void SearchAssortmentPrices(string text)
		{
			CancelLayout();
			uint id;
			UInt32.TryParse(text, out id);
			PropertyBag["prices"] = Price.Queryable
				.Where(p => (p.Supplier.Name.Contains(text) || p.Supplier.Id == id) && p.PriceType == 1)
				.OrderBy(p => p.Supplier.Name)
				.Take(50)
				.ToList();
		}

		public void MoveUserOrAddress(uint clientId, uint userId, uint addressId, uint legalEntityId, bool moveAddress)
		{
			var newClient = Client.Find(clientId);
			var address = Address.TryFind(addressId);
			var user = User.TryFind(userId);
			var legalEntity = LegalEntity.TryFind(legalEntityId);
			if (legalEntity == null)
				legalEntity = newClient.Payer.JuridicalOrganizations.Single();

			// Если нужно перенести вместе с пользователем,
			// адрес привязан только к этому пользователю и у пользователя нет других адресов,
			// тогда переносим пользователя))

			if ((user != null && user.AvaliableAddresses.Count > 1)
				|| (address != null && address.AvaliableForUsers.Count > 1))
			{
				if (moveAddress)
				{
					Flash["Message"] = Message.Error("Адрес доставки не может быть перемещен, т.к. имеет доступ к нему подключены пользователи");
					RedirectUsingRoute("deliveries", "Edit", new { id = address.Id });
					return;
				}
				else
				{
					Flash["Message"] = Message.Error("Пользователь не может быть перемещен т.к. имеет доступ к адресам доставки");
					RedirectUsingRoute("users", "Edit", new { login = user.Login });
					return;
				}
			}

			if (address != null)
				user = address.AvaliableForUsers.SingleOrDefault();
			if (user != null)
				address = user.AvaliableAddresses.SingleOrDefault();

			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();

				if (user != null)
					user.MoveToAnotherClient(newClient, legalEntity);
				if (address != null)
					address.MoveToAnotherClient(newClient, legalEntity);

				scope.VoteCommit();
			}

			if (moveAddress)
			{
				Flash["Message"] = Message.Notify("Адрес доставки успешно перемещен");
				RedirectUsingRoute("deliveries", "Edit", new { id = address.Id });
			}
			else
			{
				Flash["Message"] = Message.Notify("Пользователь успешно перемещен");
				RedirectUsingRoute("users", "Edit", new { login = user.Login });
			}
		}
	}
}