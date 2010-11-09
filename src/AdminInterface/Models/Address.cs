using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
#if !DEBUG
using System.Security.AccessControl;
#endif
using System.Threading;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.Web.Ui.Helpers;
using log4net;
using Common.Web.Ui.Models;
using AdminInterface.Models.Billing;

namespace AdminInterface.Models
{
	[ActiveRecord("Addresses", Schema = "Future", Lazy = true)]
	public class Address : ActiveRecordLinqBase<Address>, IEnablable
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property("Address")]
		public virtual string Value { get; set; }

		[BelongsTo("ClientId")]
		public virtual Client Client { get; set; }

		[BelongsTo("ContactGroupId", Lazy = FetchWhen.OnInvoke)]
		public virtual ContactGroup ContactGroup { get; set; }

		[Property]
		public virtual  bool Enabled { get; set; }

		[Property("Free")]
		public virtual  bool FreeFlag { get; set; }

		[BelongsTo("LegalEntityId", Lazy = FetchWhen.OnInvoke)]
		public virtual LegalEntity LegalEntity { get; set; }

		[BelongsTo("PayerId")]
		public virtual Payer Payer { get; set; }

		[BelongsTo("AccountingId", Cascade = CascadeEnum.All, Lazy = FetchWhen.OnInvoke)]
		public virtual Accounting Accounting { get; set; }

		[HasAndBelongsToMany(typeof (User),
			Lazy = true,
			ColumnKey = "AddressId",
			Table = "UserAddresses",
			Schema = "future",
			ColumnRef = "UserId")]
		public virtual IList<User> AvaliableForUsers { get; set; }

		public virtual string Name
		{
			get { return Value; }
		}

		public virtual string LegalName
		{
			get { return LegalEntity.Name; }
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
				return (AvaliableForUsers.Where(user => user.Enabled && (user.Client.Status == ClientStatus.On)).Count() > 0);
			}
		}

		public virtual bool IsFree
		{
			get
			{				
				return (FreeFlag || !HasPaidUsers);
			}
		}

		public virtual bool HasPaidUsers
		{
			get
			{
				// Кол-во пользователей, которым доступен этот адрес и которые включены работают НЕ бесплатно, должно быть НЕ нулевым
				return (AvaliableForUsers != null && AvaliableForUsers.Where(user => !user.IsFree && user.Enabled).Count() > 0);
			}
		}

		/// <summary>
		/// true - адрес активен (активен хотя бы один пользователь, которому доступен этот адрес)
		/// false - адрес неактивен (неактивны все пользователи, которым доступен этот адрес)
		/// </summary>
		public virtual bool IsActive
		{
			get
			{
				return (AvaliableForUsers.Where(user => user.IsActive).Count() > 0);
			}
		}

		public virtual void MaitainIntersection()
		{
			ArHelper.WithSession(s => {
				s.CreateSQLQuery(@"
set @skip = 1;

insert into Future.Intersection(ClientId, RegionId, PriceId, LegalEntityId, CostId, AvailableForClient, AgencyEnabled, PriceMarkup)
select i.ClientId, i.RegionId, i.PriceId, :legalEntityId, i.CostId, i.AvailableForClient, i.AgencyEnabled, i.PriceMarkup
from Future.Intersection i
left join Future.Intersection li on li.ClientId = i.ClientId and i.RegionId = li.RegionId and i.PriceId = li.PriceId and li.LegalEntityId = :legalEntityId
where i.clientId = :clientId and li.Id is null;

insert into Future.AddressIntersection(AddressId, IntersectionId)
select a.Id, i.Id
from Future.Intersection i
	join Future.Addresses a on a.ClientId = i.ClientId and i.LegalEntityID = a.LegalEntityId
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
			var ftpRoot = ConfigurationManager.AppSettings["FtpRoot"];
			var clientRoot = Path.Combine(ftpRoot, Id.ToString());
			try
			{
				Directory.CreateDirectory(clientRoot);

				Directory.CreateDirectory(Path.Combine(clientRoot, "Orders"));
				Directory.CreateDirectory(Path.Combine(clientRoot, "Docs"));
				Directory.CreateDirectory(Path.Combine(clientRoot, "Rejects"));
				Directory.CreateDirectory(Path.Combine(clientRoot, "Waybills"));
				foreach (var user in Client.Users)
					SetAccessControl(user.Login);
			}
			catch(Exception e)
			{
				LogManager.GetLogger(GetType()).Error(String.Format(@"
Ошибка при создании папки на ftp для клиента, иди и создавай руками
Нужно создать папку {0}
А так же создать под папки Orders, Docs, Rejects, Waybills
Дать логину {1} право читать, писать и получать список директорий и удалять под директории в папке Orders",
					clientRoot, Client.Users.First().Login), e);
			}
		}

		public virtual void SetAccessControl(string username)
		{
			if (!ADHelper.IsLoginExists(username))
				return;

			while (true)
			{
				var index = 0;
				try
				{
#if !DEBUG
					var ftpRoot = ConfigurationManager.AppSettings["FtpRoot"];
					var clientRoot = Path.Combine(ftpRoot, Id.ToString());

					username = String.Format(@"ANALIT\{0}", username);
					var rootDirectorySecurity = Directory.GetAccessControl(clientRoot);
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
					Directory.SetAccessControl(clientRoot, rootDirectorySecurity);

					var orders = Path.Combine(clientRoot, "Orders");
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
			using (var scope = new TransactionScope())
			{
				var groupOwner = Client.ContactGroupOwner;
				var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
				group.Save();
				ContactGroup = group;
			}
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

		public virtual void MoveToAnotherClient(Client newOwner, LegalEntity legalEntity)
		{
			MoveAddressIntersection(newOwner, legalEntity);

			Client = newOwner;
			Payer = LegalEntity.Payer;
			LegalEntity = LegalEntity;

			Update();
		}

		private void MoveAddressIntersection(Client newClient, LegalEntity newLegalEntity)
		{
			ArHelper.WithSession(session => session.CreateSQLQuery(@"
insert into Future.AddressIntersection(AddressId, IntersectionId, SupplierDeliveryId, ControlMinReq, MinReq)
select :AddressId, ni.Id, ai.SupplierDeliveryId, ai.ControlMinReq, ai.MinReq
from Future.Intersection ni
left join Future.Intersection oi on oi.PriceId = ni.PriceId and oi.RegionId = ni.RegionId and oi.ClientId = :OldClientId and oi.LegalEntityId = :oldLegalEntityId
left join Future.AddressIntersection ai on oi.Id = ai.IntersectionId and ai.AddressId = :AddressId
where ni.ClientId = :NewClientId and ni.LegalEntityId = :legalEntityId
;

delete future.ai
from Future.AddressIntersection ai
join Future.Intersection i on i.Id = ai.IntersectionId
where ai.AddressId = :AddressId
and i.ClientId = :OldClientId
;
")
				.SetParameter("AddressId", Id)
				.SetParameter("NewClientId", newClient.Id)
				.SetParameter("OldClientId", Client.Id)
				.SetParameter("legalEntityId", LegalEntity.Id)
				.SetParameter("oldLegalEntityId", newLegalEntity.Id)
				.ExecuteUpdate());
		}

		public virtual bool CanBeMoved()
		{
			return (AvaliableForUsers.Count == 1) &&
				(AvaliableForUsers[0].AvaliableAddresses.Count == 1);
		}
	}
}
