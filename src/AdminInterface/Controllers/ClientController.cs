using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Models.Telephony;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	public class Sort
	{
		public string Property;
		public string Direction;

		public Sort(string property, string direction)
		{
			var columns = new [] {
				"Messages.WriteTime",
				"Messages.Operator",
				"Messages.Login",

				"Addresses.Value",
				"Addresses.LegalName",

				"Users.Id",
				"Users.Login",
			};

			if (columns.All(s => !s.Equals(property, StringComparison.OrdinalIgnoreCase)))
				property = null;

			var directions = new [] {"asc", "desc"};
			if (directions.All(s => !s.Equals(direction, StringComparison.OrdinalIgnoreCase)))
				direction = null;

			Property = property;
			Direction = direction;

			if (Property == null)
				Property = "Messages.WriteTime";

			if (Direction == null)
				Direction = "Desc";
		}

		public void Apply(IDictionary bag)
		{
			var indexOf = Property.IndexOf(".");
			var prefix = Property.Substring(0, indexOf);
			if (!bag.Contains(prefix))
				return;

			indexOf++;
			var property = Property.Substring(indexOf, Property.Length - indexOf);

			var sortedList = new ArrayList(((IList)bag[prefix]));
			sortedList.Sort(
				new PropertyComparer(Direction.Equals("asc", StringComparison.OrdinalIgnoreCase) ? SortDirection.Asc : SortDirection.Desc, property)
			);

			bag[prefix] = sortedList;
			bag["SortBy"] = Property;
			bag["Direction"] = Direction;
		}
	}

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
	public class ClientController : ARController
	{
		public void Show(uint id)
		{
			var sort = GetSort();
			var client = Client.FindAndCheck(id);
			var users = client.Users;
			var addresses = client.Addresses;

			PropertyBag["Client"] = client;
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
			PropertyBag["users"] = users.OrderBy(user => user.Id).ToList();
			PropertyBag["addresses"] = addresses.OrderBy(a => a.LegalEntity.Name).ThenBy(a => a.Name).ToList();

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["messages"] = ClientInfoLogEntity.MessagesForClient(client);
			PropertyBag["usersInfo"] = ADHelper.GetPartialUsersInformation(users);

			sort.Apply(PropertyBag);
		}

		private Sort GetSort()
		{
			return new Sort(Query["SortBy"], Query["Direction"]);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client)
		{
			Administrator.CheckClientPermission(client);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				client.Save();
			}

			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void UpdateDrugstore(
			[ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client,
			[ARDataBind("drugstore", AutoLoad = AutoLoadBehavior.Always)] DrugstoreSettings drugstore,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			ulong homeRegion,
			bool activateBuyMatrix)
		{
			Administrator.CheckClientPermission(client);
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var oldMaskRegion = client.MaskRegion;
				client.HomeRegion = Region.Find(homeRegion);
				client.UpdateRegionSettings(regionSettings);
				if (!activateBuyMatrix)
					drugstore.BuyingMatrixPrice = null;
				if (drugstore.EnableSmartOrder && drugstore.SmartOrderRules == null)
				{
					var smartOrder = SmartOrderRules.TestSmartOrder();
					drugstore.SmartOrderRules = smartOrder;
				}
				client.Save();
				drugstore.UpdateAndFlush();
				if (oldMaskRegion != client.MaskRegion)
					client.MaintainIntersection();

				scope.VoteCommit();
			}
			Flash["Message"] = Message.Notify("Сохранено");
			RedirectToUrl(String.Format("../client/{0}", client.Id));
		}

		public void SuppliersForCostNoising(uint clientId)
		{
			CancelLayout();
			var client = Client.FindAndCheck(clientId);
			var suppliers = client.Payers.SelectMany(p => Supplier.GetByPayerId(p.Id)).OrderBy(s => s.Name).ToList();
			PropertyBag["suppliers"] = suppliers;
			if (client.Settings.NoiseCostExceptSupplier != null)
				PropertyBag["FirmCodeOnly"] = client.Settings.NoiseCostExceptSupplier.Id;
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
					new ClientInfoLogEntity(message, client).Save();

				Flash["Message"] = Message.Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		public void Unlock(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			foreach(var user in client.Users)
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
				foreach (var user in client.Users)
				{
					var file = String.Format(Properties.Settings.Default.UserPreparedDataFormatString, user.Id);
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
						inUser = Administrator.UserName,
						ResetIdCause = reason
					});
				ClientInfoLogEntity.ReseteUin(client, reason).Save();
				client.ResetUin();
				RedirectToReferrer();
			}
		}

		[AccessibleThrough(Verb.Get)]
		public void Settings(uint id)
		{
			var client = Client.Find(id);
			var regions = Region.FindAll().OrderBy(region => region.Name).ToArray();
			var drugstore = client.Settings;
			PropertyBag["client"] = client;
			PropertyBag["regions"] = regions;
			PropertyBag["drugstore"] = drugstore;
			if (drugstore.BuyingMatrixPrice != null)
				PropertyBag["BuyMatrixPrice"] = drugstore.BuyingMatrixPrice;
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
			return client.Orgs().Select(j => new {j.Id, j.Name}).ToArray();
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

		public void UpdateClientStatus(uint clientId, bool enabled)
		{
			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging();
				var client = Client.Find(clientId);
				var oldEnabled = client.Enabled;
				client.Enabled = enabled;
				if (oldEnabled != client.Enabled)
				{
					this.Mail().EnableChanged(client, enabled).Send();
					ClientInfoLogEntity.StatusChange(client.Status, client).Save();
				}
				client.Save();
			}
			CancelView();
			CancelLayout();
		}

		public void MoveUserOrAddress(uint clientId, uint userId, uint addressId, uint legalEntityId, bool moveAddress)
		{
			var newClient = Client.Find(clientId);
			var address = Address.TryFind(addressId);
			var user = User.TryFind(userId);

			Client oldClient = null;
			if (user != null)
				oldClient = user.Client;
			if (address != null)
				oldClient = address.Client;

			var legalEntity = LegalEntity.TryFind(legalEntityId);
			if (legalEntity == null)
				legalEntity = newClient.Orgs().Single();

			// Если нужно перенести вместе с пользователем,
			// адрес привязан только к этому пользователю и у пользователя нет других адресов,
			// тогда переносим пользователя

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
					RedirectUsingRoute("users", "Edit", new { id = user.Id });
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
				RedirectUsingRoute("users", "Edit", new { id = user.Id });
			}
			oldClient.Refresh();
			if (oldClient.Users.Count == 0 && oldClient.Addresses.Count == 0)
				UpdateClientStatus(oldClient.Id, false);
			oldClient.Save();
		}
	}
}