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
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
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

		public void Show(uint id)
		{
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

			Sort.Make(this);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client)
		{
			Admin.CheckClientPermission(client);
			client.Save();
			Notify("Сохранено");
			RedirectToReferrer();
		}

		[AccessibleThrough(Verb.Post)]
		public void UpdateDrugstore(
			[ARDataBind("client", AutoLoad = AutoLoadBehavior.Always)] Client client,
			[ARDataBind("drugstore", AutoLoad = AutoLoadBehavior.Always, Expect = "drugstore.OfferMatrixExcludes")] DrugstoreSettings drugstore,
			[DataBind("regionSettings")] RegionSettings[] regionSettings,
			ulong homeRegion)
		{
			Admin.CheckClientPermission(client);

			var oldMaskRegion = client.MaskRegion;
			client.HomeRegion = Region.Find(homeRegion);
			client.UpdateRegionSettings(regionSettings);
				
			if (drugstore.EnableSmartOrder && drugstore.SmartOrderRules == null)
			{
				var smartOrder = SmartOrderRules.TestSmartOrder();
				drugstore.SmartOrderRules = smartOrder;
			}
			client.Save();
			drugstore.UpdateAndFlush();
			if (oldMaskRegion != client.MaskRegion)
				client.MaintainIntersection();

			Notify("Сохранено");
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
				new ClientInfoLogEntity(message, client).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		public void Unlock(uint clientCode)
		{
			var client = Client.FindAndCheck(clientCode);

			foreach(var user in client.Users)
				if (ADHelper.IsLoginExists(user.Login) && ADHelper.IsLocked(user.Login))
					ADHelper.Unlock(user.Login);

			Notify("Разблокировано");
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
			var client = Client.FindAndCheck(clientCode);

			using (new TransactionScope())
			{
				DbLogHelper.SetupParametersForTriggerLogging(
					new {
						inHost = Request.UserHostAddress,
						inUser = Admin.UserName,
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
			var regions = Region.All().ToArray();
			var drugstore = client.Settings;
			PropertyBag["client"] = client;
			PropertyBag["regions"] = regions;
			PropertyBag["drugstore"] = drugstore;
		}

		public void NotifySuppliers(uint clientId)
		{
			var client = Client.Find(clientId);
			new NotificationService().NotifySupplierAboutDrugstoreRegistration(client, true);
			Notify("Уведомления отправлены");
			RedirectToReferrer();
		}

		public void SearchClient(string searchText)
		{
			CancelLayout();
			int searchNumber;
			Int32.TryParse(searchText, out searchNumber);
			PropertyBag["clients"] = Client.Queryable.Where(c => c.Name.Contains(searchText) || c.Id == searchNumber).OrderBy(c => c.Name).ToList();
			RenderView("SearchClientSubview");
		}

		public void ChangePayer(uint clientId, uint payerId, uint orgId)
		{
			var client = Client.FindAndCheck(clientId);
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
			var client = Client.FindAndCheck(id);
			return client.Orgs().Select(j => new {j.Id, j.Name}).ToArray();
		}

		[return: JSONReturnBinder]
		public object[] SearchAssortmentPrices(string text)
		{
			uint id;
			UInt32.TryParse(text, out id);
			return Price.Queryable
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
		public object[] SearchSuppliers(uint id, string text)
		{
			var client = Client.Find(id);
			var suppliers = ArHelper.WithSession(s => {
				s.CreateSQLQuery(@"call future.GetPrices(:userid)")
					.SetParameter("userid", client.Users.First().Id)
					.ExecuteUpdate();

				return s.CreateSQLQuery(@"
select s.Id, s.Name from Prices ap
join Future.Suppliers s on s.Id = ap.FirmCode
where s.Name like :SearchText")
						.SetParameter("SearchText", "%" + text + "%")
						.List();
			});
			return suppliers
				.Cast<object[]>()
				.Select(s => new { id = Convert.ToUInt32(s[0]), name = Convert.ToString(s[1])})
				.ToArray();
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
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				DbLogHelper.SetupParametersForTriggerLogging();

				if (user != null)
					user.MoveToAnotherClient(newClient, legalEntity);
				if (address != null)
					address.MoveToAnotherClient(newClient, legalEntity);
				scope.VoteCommit();
			}

			if (address != null)
			{
				this.Mailer()
					.AddressMoved(address, oldClient, address.OldValue(a => a.LegalEntity))
					.Send();
			}

			if (user != null)
				this.Mailer()
					.UserMoved(user, oldClient, user.OldValue(u => u.Payer))
					.Send();

			if (moveAddress)
			{
				Notify("Адрес доставки успешно перемещен");
				RedirectUsingRoute("deliveries", "Edit", new { id = address.Id });
			}
			else
			{
				Notify("Пользователь успешно перемещен");
				RedirectUsingRoute("users", "Edit", new { id = user.Id });
			}
			oldClient.Refresh();
			if (oldClient.Users.Count == 0
				&& oldClient.Addresses.Count == 0
				&& oldClient.Enabled)
			{
				oldClient.Disabled = true;
				this.Mailer().EnableChanged(oldClient).Send();
				ClientInfoLogEntity.StatusChange(oldClient).Save();
			}
			oldClient.Save();
		}
	}
}