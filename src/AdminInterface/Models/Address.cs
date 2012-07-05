using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
//#if !DEBUG
using System.Security.AccessControl;
//#endif
using System.Threading;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Tools;
using Common.Web.Ui.Helpers;
using log4net;
using Common.Web.Ui.Models;
using AdminInterface.Models.Billing;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Customers", Lazy = true), Auditable]
	public class Address : ActiveRecordLinqBase<Address>, IEnablable, IDisabledByParent
	{
		private bool _enabled;

		public Address(Client client, LegalEntity legalEntity)
			: this()
		{
			Client = client;
			LegalEntity = legalEntity;
			Payer = legalEntity.Payer;
		}

		public Address()
		{
			AvaliableForUsers = new List<User>();
		}

		public Address(Client client)
			: this()
		{
			Client = client;
			Payer = client.Payers.First();
			LegalEntity = Payer.JuridicalOrganizations.First();
			Accounting = new AddressAccount(this);
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property("Address"), Description("Адрес"), Auditable, ValidateNonEmpty]
		public virtual string Value { get; set; }

		[BelongsTo("ClientId"), Description("Клиент"), Auditable]
		public virtual Client Client { get; set; }

		[BelongsTo("ContactGroupId", Lazy = FetchWhen.OnInvoke, Cascade = CascadeEnum.All)]
		public virtual ContactGroup ContactGroup { get; set; }

		[Property(Access = PropertyAccess.FieldCamelcaseUnderscore), Description("Включен"), Auditable]
		public virtual bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if (_enabled != value)
				{
					if (Payer != null)
						Payer.PaymentSum = Payer.TotalSum;
					_enabled = value;
				}
			}
		}

		[Nested]
		public virtual RegistrationInfo Registration { get; set; }

		[BelongsTo("LegalEntityId", Lazy = FetchWhen.OnInvoke), Description("Юр.лицо"), Auditable]
		public virtual LegalEntity LegalEntity { get; set; }

		[BelongsTo("PayerId"), Description("Плательщик"), Auditable]
		public virtual Payer Payer { get; set; }

		[BelongsTo("AccountingId", Cascade = CascadeEnum.All, Lazy = FetchWhen.OnInvoke)]
		public virtual Account Accounting { get; set; }

		[HasAndBelongsToMany(typeof (User),
			Lazy = true,
			ColumnKey = "AddressId",
			Table = "UserAddresses",
			Schema = "Customers",
			ColumnRef = "UserId"), Auditable("список пользователей, подключеных к адресу доставки")]
		public virtual IList<User> AvaliableForUsers { get; set; }

		public virtual string Name
		{
			get { return Value; }
		}

		public virtual string LegalName
		{
			get { return LegalEntity.Name; }
		}

		[Style]
		public virtual bool Disabled
		{
			get { return !Enabled; }
		}

		[Style]
		public virtual bool DisabledByParent
		{
			get { return Client != null && Client.Disabled; }
		}

		public virtual bool AvaliableFor(User user)
		{
			return AvaliableForUsers.Any(u => u.Id == user.Id);
		}

		/// <summary>
		/// true, если доступен хотя бы одному включенному пользователю
		/// false, если доступен только отключенным пользователям
		/// </summary>
		public virtual bool AvaliableForEnabledUsers
		{
			get
			{
				return AvaliableForUsers.Any(user => user.Enabled && (user.Client.Status == ClientStatus.On));
			}
		}

		[Style]
		public virtual bool HasNoConnectedUsers
		{
			get { return AvaliableForUsers.Count == 0; }
		}

		[Style]
		public virtual bool IsOldUserUpdate
		{
			get
			{
				if (AvaliableForUsers.Count == 0)
					return false;
				return AvaliableForUsers.All(u => u.IsOldUserUpdate);
			}
		}

		public virtual void Maintain()
		{
			MaintainInscribe();
			MaintainIntersection();
		}

		public virtual void MaintainIntersection()
		{
			ArHelper.WithSession(s => {
				s.CreateSQLQuery(@"
set @skip = 1;

insert into Customers.Intersection(ClientId, RegionId, PriceId, LegalEntityId, CostId, AvailableForClient, AgencyEnabled, PriceMarkup)
select i.ClientId, i.RegionId, i.PriceId, :legalEntityId, i.CostId, i.AvailableForClient, i.AgencyEnabled, i.PriceMarkup
from Customers.Intersection i
left join Customers.Intersection li on li.ClientId = i.ClientId and i.RegionId = li.RegionId and i.PriceId = li.PriceId and li.LegalEntityId = :legalEntityId
where i.clientId = :clientId and li.Id is null
group by i.ClientId, i.RegionId, i.PriceId;

insert into Customers.AddressIntersection(AddressId, IntersectionId)
select a.Id, i.Id
from Customers.Intersection i
	join Customers.Addresses a on a.ClientId = i.ClientId and i.LegalEntityID = a.LegalEntityId
where a.Id = :addressId
;
set @skip = 0;
")
					.SetParameter("addressId", Id)
					.SetParameter("legalEntityId", LegalEntity.Id)
					.SetParameter("clientId", Client.Id)
					.ExecuteUpdate();
			});
		}

		public virtual void CreateFtpDirectory()
		{
			CreateFtpDirectory(GetAddressRoot());
		}

		public virtual void CreateFtpDirectory(string addressRoot)
		{
			try
			{
				Directory.CreateDirectory(addressRoot);

				Directory.CreateDirectory(Path.Combine(addressRoot, "Orders"));
				Directory.CreateDirectory(Path.Combine(addressRoot, "Docs"));
				Directory.CreateDirectory(Path.Combine(addressRoot, "Rejects"));
				Directory.CreateDirectory(Path.Combine(addressRoot, "Waybills"));
			}
			catch(Exception e)
			{
				LogManager.GetLogger(GetType()).Error(String.Format(@"
Ошибка при создании папки на ftp для клиента, иди и создавай руками
Нужно создать папку {0}
А так же создать под папки Orders, Docs, Rejects, Waybills
Дать логинам {1} право читать, писать и получать список директорий и удалять под директории в папке Orders",
					addressRoot, Client.Users.Implode(u => u.Login)), e);
			}
		}

		public virtual void SetAccessControl(string username)
		{
			SetAccessControl(username, GetAddressRoot());
		}

		private string GetAddressRoot()
		{
			return Path.Combine(ConfigurationManager.AppSettings["AptBox"], Id.ToString());
		}

		public virtual void SetAccessControl(string username, string root)
		{
			if (!ADHelper.IsLoginExists(username))
				return;

			var index = 0;
			while (true)
			{
				try
				{
#if !DEBUG
					username = String.Format(@"ANALIT\{0}", username);
					var rootDirectorySecurity = Directory.GetAccessControl(root);
					rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
						FileSystemRights.Read,
						InheritanceFlags.ContainerInherit |
							InheritanceFlags.ObjectInherit,
						PropagationFlags.None,
						AccessControlType.Allow));
					rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
						FileSystemRights.Write,
						InheritanceFlags.ContainerInherit |
							InheritanceFlags.ObjectInherit,
						PropagationFlags.None,
						AccessControlType.Allow));
					rootDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
						FileSystemRights.ListDirectory,
						InheritanceFlags.ContainerInherit |
							InheritanceFlags.ObjectInherit,
						PropagationFlags.None,
						AccessControlType.Allow));
					Directory.SetAccessControl(root, rootDirectorySecurity);

					var orders = Path.Combine(root, "Orders");
					if (Directory.Exists(orders))
					{
						var ordersDirectorySecurity = Directory.GetAccessControl(orders);
						ordersDirectorySecurity.AddAccessRule(new FileSystemAccessRule(username,
							FileSystemRights.DeleteSubdirectoriesAndFiles,
							InheritanceFlags.ContainerInherit |
								InheritanceFlags.ObjectInherit,
							PropagationFlags.None,
							AccessControlType.Allow));
						Directory.SetAccessControl(orders, ordersDirectorySecurity);
					}
#endif
					break;
				}
				catch(Exception e)
				{
					LogManager.GetLogger(typeof(Address)).Error("Ошибка при назначении прав, пробую еще раз", e);
					index++;
					Thread.Sleep(500);
					if (index > 3)
						break;
				}
			}
		}

		private void AddContactGroup()
		{
			var groupOwner = Client.ContactGroupOwner;
			var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
			group.Save();
			ContactGroup = group;
		}

		public virtual void UpdateContacts(Contact[] displayedContacts, Contact[] deletedContacts)
		{
			if (ContactGroup == null)
				AddContactGroup();
			ContactGroup.UpdateContacts(displayedContacts, deletedContacts);
		}

		public virtual void UpdateContacts(Contact[] displayedContacts)
		{
			UpdateContacts(displayedContacts, null);
		}

		public virtual void MoveToAnotherClient(Client newOwner, LegalEntity newLegalEntity)
		{
			if (!newOwner.Orgs().Any(o => o.Id == newLegalEntity.Id))
				throw new Exception(String.Format("Не могу переместить адрес {0} т.к. юр. лицо {1} не принадлежит клиенту {2}",
					this, newLegalEntity, newOwner));

			Maintainer.MaintainIntersection(newOwner, newLegalEntity);
			MoveAddressIntersection(newOwner, newLegalEntity,
				Client, LegalEntity);

			Client = newOwner;
			Payer = newLegalEntity.Payer;
			LegalEntity = newLegalEntity;
			ClientInfoLogEntity.UpdateLogs(newOwner.Id, Id);
			Update();
		}

		public virtual void MoveAddressIntersection(Client newClient, LegalEntity newLegalEntity, 
			Client oldClient, LegalEntity oldLegalEntity)
		{
			ArHelper.WithSession(session => session.CreateSQLQuery(@"
insert into Customers.AddressIntersection(AddressId, IntersectionId, SupplierDeliveryId, ControlMinReq, MinReq)
select :AddressId, ni.Id, ai.SupplierDeliveryId, ai.ControlMinReq, ai.MinReq
from Customers.Intersection ni
left join Customers.Intersection oi on oi.PriceId = ni.PriceId and oi.RegionId = ni.RegionId and oi.ClientId = :OldClientId and oi.LegalEntityId = :OldLegalEntityId
left join Customers.AddressIntersection ai on oi.Id = ai.IntersectionId and ai.AddressId = :AddressId
where ni.ClientId = :NewClientId and ni.LegalEntityId = :NewLegalEntityId
;

delete ai
from Customers.AddressIntersection ai
join Customers.Intersection i on i.Id = ai.IntersectionId
where ai.AddressId = :AddressId
and i.ClientId = :OldClientId
and i.LegalEntityId = :OldLegalEntityId
;
")
				.SetParameter("AddressId", Id)
				.SetParameter("NewClientId", newClient.Id)
				.SetParameter("OldClientId", oldClient.Id)
				.SetParameter("NewLegalEntityId", newLegalEntity.Id)
				.SetParameter("OldLegalEntityId", oldLegalEntity.Id)
				.ExecuteUpdate());
		}

		public virtual bool CanBeMoved()
		{
			return (AvaliableForUsers.Count == 1) &&
				(AvaliableForUsers[0].AvaliableAddresses.Count == 1);
		}

		public virtual void MaintainInscribe()
		{
			if (Client.Settings.InvisibleOnFirm != DrugstoreType.Standart)
				return;

			ArHelper.WithSession(s => {
				s.CreateSQLQuery("insert into inscribe(AddressId) values(:AddressId);")
					.SetParameter("AddressId", Id)
					.ExecuteUpdate();
			});
		}

		public virtual void AddBillingComment(string billingMessage)
		{
			if (String.IsNullOrEmpty(billingMessage))
				return;

			new ClientInfoLogEntity("Сообщение в биллинг: " + billingMessage, this).Save();
			billingMessage = String.Format("О регистрации Адреса {0} для клиента {1} ( {2} ): {3}", Name, Client.Name, Client.Id, billingMessage);
			Payer.AddComment(billingMessage);
		}

		public virtual bool CanDelete()
		{
			return ActiveRecordLinqBase<ClientOrder>.Queryable.Count(o => o.Address == this) == 0;
		}

		public override void Delete()
		{
			Payer.Addresses.Remove(this);
			Client.Addresses.Remove(this);
			ClientInfoLogEntity.DeleteAuditRecords(this);
			PayerAuditRecord.DeleteAuditRecords(Accounting);

			base.Delete();
		}
	}
}
