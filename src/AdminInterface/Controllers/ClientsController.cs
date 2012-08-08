using System;
using System.Collections;
using System.Collections.Specialized;
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
using AdminInterface.NHibernateExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;

namespace AdminInterface.Controllers
{
	public class Sort
	{
		public string Property;
		public string Direction;

		public Sort(NameValueCollection query)
			: this(query["SortBy"], query["Direction"])
		{}

		public Sort(string property, string direction)
		{
			var columns = new [] {
				"Messages.WriteTime",
				"Messages.Operator",
				"Messages.Type",
				"Messages.ObjectId",
				"Messages.Name",

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

		public static void Make(SmartDispatcherController controller)
		{
			new Sort(controller.Query).Apply(controller.PropertyBag);
		}
	}

	[
		Helper(typeof(ADHelper)),
		Helper(typeof(ViewHelper)),
		Helper(typeof(HttpUtility)),
		Rescue("Fail", typeof(LoginNotFoundException)),
		Rescue("Fail", typeof(CantChangePassword)),
		Secure(PermissionType.ViewDrugstore),
		Filter(ExecuteWhen.BeforeAction, typeof(SecurityActivationFilter))
	]
	public class ClientsController : AdminInterfaceController
	{
		public ClientsController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Show(uint id, [SmartBinder(Expect = "filter.Types")] MessageQuery filter)
		{
			var client = Service.FindAndCheck<Client>(id);
			var users = client.Users;
			var addresses = client.Addresses;

			PropertyBag["Client"] = client;
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
			PropertyBag["users"] = users.OrderBy(user => user.Id).ToList();
			PropertyBag["addresses"] = addresses.OrderBy(a => a.LegalEntity.Name).ThenBy(a => a.Name).ToList();

			PropertyBag["usersInfo"] = ADHelper.GetPartialUsersInformation(users);

			PropertyBag["filter"] = filter;
			PropertyBag["messages"] = filter.Execute(client, DbSession);

			Sort.Make(this);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client)
		{
			Admin.CheckClientPermission(client);

			var savedNotify = true;
			var changeName = client.IsChanged(c => c.Name);
			var changeFullName = client.IsChanged(c => c.FullName);
			if (changeFullName || changeName) {
				var legalEntityes = client.GetLegalEntity();
				if (legalEntityes.Count == 1) {
					var legalEntity = legalEntityes.First();
					if (changeName)
						legalEntity.Name = client.Name;
					if (changeFullName)
						legalEntity.FullName = client.FullName;
					legalEntity.Update();
				}
				else {
					var changePartMessage = string.Empty;
					if (changeName)
						changePartMessage += "краткое";
					if (changeFullName)
						changePartMessage += "полное";
					Notify(string.Format("Вы изменили {0} наименование клиента. У клиента более одного юр. лица, переименование юр. лиц не было произведено.", changePartMessage));
					savedNotify = false;
				}
			}
			DbSession.SaveOrUpdate(client);
			if (savedNotify)
				Notify("Сохранено");
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void BindPhone(uint id, string phone)
		{
			var owner = DbSession.Load<ContactGroupOwner>(id);
			var group = owner.ContactGroups.FirstOrDefault(c => c.Type == ContactGroupType.KnownPhones);
			if (group == null)
				group = owner.AddContactGroup(ContactGroupType.KnownPhones);
			phone = phone.Substring(0, 4) + "-" + phone.Substring(4, phone.Length - 4);
			group.AddContact(new Contact{ ContactText = phone, Type = ContactType.Phone});

			DbSession.CreateSQLQuery(@"
delete from telephony.UnresolvedPhone
where Phone like :phone")
				.SetParameter("phone", phone.Replace("-", ""))
				.ExecuteUpdate();

			group.Save();
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void SendMessage(string message, uint clientCode)
		{
			var client = Client.FindAndCheck<Client>(clientCode);

			if (!String.IsNullOrEmpty(message))
			{
				new AuditRecord(message, client).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		public void Unlock(uint clientCode)
		{
			var client = Client.FindAndCheck<Client>(clientCode);

			foreach(var user in client.Users)
				if (ADHelper.IsLoginExists(user.Login) && ADHelper.IsLocked(user.Login))
					ADHelper.Unlock(user.Login);

			Notify("Разблокировано");
			RedirectToReferrer();
		}

		public void DeletePreparedData(uint clientCode)
		{
			var client = Client.FindAndCheck<Client>(clientCode);

			try
			{
				foreach (var user in client.Users)
				{
					var file = String.Format(Properties.Settings.Default.UserPreparedDataFormatString, user.Id);
					if (File.Exists(file))
						File.Delete(file);
				}
				Notify("Подготовленные данные удалены");
			}
			catch
			{
				Error("Ошибка удаления подготовленных данных, попробуйте позднее.");
			}
			RedirectToReferrer();
		}

		public void ResetUin(uint clientCode, string reason)
		{
			var client = Client.FindAndCheck<Client>(clientCode);
			AuditRecord.ReseteUin(client, reason).Save();
			client.ResetUin();
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Get)]
		public void Settings(uint id)
		{
			var client = Client.FindAndCheck<Client>(id);
			var regions = Region.All().ToArray();
			var drugstore = client.Settings;
			PropertyBag["client"] = client;
			PropertyBag["regions"] = regions;
			PropertyBag["drugstore"] = drugstore;
		}

		[AccessibleThrough(Verb.Post)]
		public void UpdateDrugstore(
			[ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client,
			[ARDataBind("drugstore.SmartOrderRules", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] SmartOrderRules smartOrderRules,
			[ARDataBind("drugstore", AutoLoad = AutoLoadBehavior.NullIfInvalidKey, Expect = "drugstore.OfferMatrixExcludes")] DrugstoreSettings drugstore,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			ulong homeRegion)
		{
			Admin.CheckClientPermission(client);

			if (Form["ResetReclameDate"] != null) {
				new ResetReclameDate(client).Execute(DbSession);
				Notify("Сброшена");
				RedirectTo(client);
				return;
			}

			var oldMaskRegion = client.MaskRegion;
			client.HomeRegion = Region.Find(homeRegion);
			client.UpdateRegionSettings(regionSettings);

			if (!IsValid(client)) {
				Settings(client.Id);
				PropertyBag["client"] = client;
				RenderView("Settings");
				return;
			}

			if (drugstore.EnableSmartOrder) {
				if (drugstore.SmartOrderRules == null && smartOrderRules == null) {
					var smartOrder = SmartOrderRules.TestSmartOrder();
					drugstore.SmartOrderRules = smartOrder;
				}
				else {
					drugstore.SmartOrderRules = smartOrderRules;
					var parseAlgorithm = drugstore.SmartOrderRules.ParseAlgorithm;
					uint algorithmId;
					if (UInt32.TryParse(parseAlgorithm, out algorithmId)) {
						var algorithm = DbSession.Load<ParseAlgorithm>(algorithmId);
						if (algorithm != null)
							drugstore.SmartOrderRules.ParseAlgorithm = algorithm.Name;
					}
				}
			}
			DbSession.SaveOrUpdate(client);
			DbSession.SaveOrUpdate(drugstore);
			DbSession.Flush();
			if (oldMaskRegion != client.MaskRegion)
				client.MaintainIntersection();

			Notify("Сохранено");
			RedirectTo(client);
		}

		public void NotifySuppliers(uint clientId)
		{
			var client = DbSession.Load<Client>(clientId);
			new NotificationService(Defaults).NotifySupplierAboutDrugstoreRegistration(client, true);
			Notify("Уведомления отправлены");
			RedirectToReferrer();
		}

		public void SearchClient(string searchText)
		{
			CancelLayout();
			int searchNumber;
			Int32.TryParse(searchText, out searchNumber);
			PropertyBag["clients"] = DbSession.Query<Client>()
				.Where(c => c.Name.Contains(searchText) || c.Id == searchNumber)
				.OrderBy(c => c.Name)
				.ToList();
			RenderView("SearchClientSubview");
		}

		public void ChangePayer(uint clientId, uint payerId, uint orgId)
		{
			var client = Client.FindAndCheck<Client>(clientId);
			var payer = Payer.Find(payerId);
			var org = payer.JuridicalOrganizations.FirstOrDefault(j => j.Id == orgId);
			client.ChangePayer(payer, org);

			Notify("Изменено");
			RedirectToAction("Show", new {id = client.Id});
		}

		[AccessibleThrough(Verb.Get)]
		[return: JSONReturnBinder]
		public object[] LegalEntities(uint id)
		{
			var client = Client.FindAndCheck<Client>(id);
			return client.Orgs().Select(j => new {j.Id, j.Name}).ToArray();
		}

		[return: JSONReturnBinder]
		public object[] SearchAssortmentPrices(string text)
		{
			uint id;
			UInt32.TryParse(text, out id);
			return DbSession.Query<Price>()
				.Where(p => (p.Supplier.Name.Contains(text) || p.Supplier.Id == id) && p.PriceType == PriceType.Assortment)
				.OrderBy(p => p.Supplier.Name)
				.Take(50)
				.ToArray()
				.Select(p => new {id = p.Id, name = String.Format("{0} - {1}", p.Supplier.Name, p.Name)})
				.ToArray();
		}

		[return: JSONReturnBinder]
		public object[] SearchPayer(string text)
		{
			return ActiveRecordLinqBase<Payer>
				.Queryable
				.Where(p => p.Name.Contains(text))
				.Take(50)
				.ToList()
				.Select(p => new {
					id = p.Id,
					name = String.Format("{0}, {1}", p.Id, p.Name)
				})
				.ToArray();
		}

		[return: JSONReturnBinder]
		public object[] GetPayerOrgs(uint id)
		{
			var payer = Payer.Find(id);
			return payer.JuridicalOrganizations.Select(o => new {
				id = o.Id,
				name = o.Name
			}).ToArray();
		}

		[return: JSONReturnBinder]
		public object[] SerachParseAlgorithm(string text)
		{
			return DbSession.QueryOver<ParseAlgorithm>().Where(
				Restrictions.On<ParseAlgorithm>(l => l.Name).IsLike(text, MatchMode.Anywhere))
				.Take(50)
				.List().OrderBy(l => l.Name)
				.Select(p => new {id = p.Id, name = p.Name})
				.ToArray();
		}

		[return: JSONReturnBinder]
		public object[] SearchSuppliers(uint id, string text)
		{
			var client = DbSession.Load<Client>(id);
			var user = client.Users.FirstOrDefault();
			if (user == null)
				return Enumerable.Empty<object>().ToArray();

			DbSession.CreateSQLQuery(@"call Customers.GetPrices(:userid)")
				.SetParameter("userid", user.Id)
				.ExecuteUpdate();

			return DbSession.CreateSQLQuery(@"
select s.Id, s.Name from Prices ap
join Customers.Suppliers s on s.Id = ap.FirmCode
where s.Name like :SearchText")
				.SetParameter("SearchText", "%" + text + "%")
				.List()
				.Cast<object[]>()
				.Select(s => new { id = Convert.ToUInt32(s[0]), name = String.Format("{0}. {1}", s[0], s[1])})
				.ToArray();
		}

		public void MoveUserOrAddress(uint clientId, uint userId, uint addressId, uint legalEntityId, bool moveAddress)
		{
			var newClient = DbSession.Load<Client>(clientId);
			var address = DbSession.Get<Address>(addressId);
			var user = DbSession.Get<User>(userId);

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
					Error("Адрес доставки не может быть перемещен, т.к. имеет доступ к нему подключены пользователи");
					RedirectUsingRoute("deliveries", "Edit", new { id = address.Id });
					return;
				}
				else
				{
					Error("Пользователь не может быть перемещен т.к. имеет доступ к адресам доставки");
					RedirectUsingRoute("users", "Edit", new { id = user.Id });
					return;
				}
			}

			if (user != null && user.AvaliableAddresses.Any(a => a.AvaliableForUsers.Count > 1)) {
					Error("Пользователь не может быть перемещен т.к. подключен к адресу который связан с другими пользователями");
					RedirectUsingRoute("users", "Edit", new { id = user.Id });
					return;
			}

			if (address != null)
				user = address.AvaliableForUsers.SingleOrDefault();
			if (user != null)
				address = user.AvaliableAddresses.SingleOrDefault();

			AuditRecord log = null;
			var query = new UpdateOrders(newClient, user, address);
			if (user != null)
				log = user.MoveToAnotherClient(newClient, legalEntity);
			if (address != null)
				log = address.MoveToAnotherClient(newClient, legalEntity);

			query.Execute(DbSession);
			DbSession.Save(log);
			//нужно сохранить изменения, иначе oldClient.Refresh(); не зафиксирует их
			DbSession.Flush();

			if (address != null)
				this.Mailer()
					.AddressMoved(address, oldClient, address.OldValue(a => a.LegalEntity))
					.Send();

			if (user != null)
				this.Mailer()
					.UserMoved(user, oldClient, user.OldValue(u => u.Payer))
					.Send();

			if (moveAddress) {
				Notify("Адрес доставки успешно перемещен");
				RedirectUsingRoute("deliveries", "Edit", new { id = address.Id });
			}
			else {
				Notify("Пользователь успешно перемещен");
				RedirectUsingRoute("users", "Edit", new { id = user.Id });
			}
			DbSession.Refresh(oldClient);
			if (oldClient.Users.Count == 0
				&& oldClient.Addresses.Count == 0
				&& oldClient.Enabled)
			{
				oldClient.Disabled = true;
				this.Mailer().EnableChanged(oldClient).Send();
				DbSession.Save(AuditRecord.StatusChange(oldClient));
			}
			DbSession.Save(oldClient);
		}

		public void Delete(uint id)
		{
			var client = Client.FindAndCheck<Client>(id);

			if (!client.CanDelete(DbSession)) {
				Error("Не могу удалить клиента т.к. у него есть заказы");
				RedirectToReferrer();
				return;
			}

			client.Delete(DbSession);
			Notify("Удалено");
			Redirect("Users", "Search");
		}
	}
}