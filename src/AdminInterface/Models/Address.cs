using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
#if !DEBUG
using System.Security.AccessControl;
#endif
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Common.MySql;
using Common.Web.Ui.Helpers;
using log4net;
using Common.Web.Ui.Models;
using AdminInterface.Models.Billing;

namespace AdminInterface.Models
{
	[ActiveRecord("Addresses", Schema = "Future")]
	public class Address : ActiveRecordLinqBase<Address>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property("Address")]
		public string Value { get; set; }

		[BelongsTo("ClientId")]
		public Client Client { get; set; }

		[BelongsTo("ContactGroupId")]
		public ContactGroup ContactGroup { get; set; }

		[Property]
		public bool Enabled { get; set; }

		[Property("Free")]
		public bool FreeFlag { get; set; }

		[Property]
		public bool BeAccounted { get; set; }

		[BelongsTo("JuridicalOrganizationId", NotNull = true, Lazy = FetchWhen.OnInvoke)]
		public JuridicalOrganization JuridicalOrganization { get; set; }

		[HasAndBelongsToMany(typeof (User),
			Lazy = true,
			ColumnKey = "AddressId",
			Table = "future.UserAddresses",
			ColumnRef = "UserId")]
		public virtual IList<User> AvaliableForUsers { get; set; }
		
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
insert into Future.AddressIntersection(AddressId, IntersectionId)
select a.Id, i.Id
from Future.Intersection i
	join Future.Addresses a on a.ClientId = i.ClientId
	left join Future.AddressIntersection ai on ai.AddressId = a.Id and ai.IntersectionId = i.Id
where a.Id = :addressId")
					.SetParameter("addressId", Id)
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

		public void SetAccessControl(string username)
		{
#if !DEBUG
			if (!ADHelper.IsLoginExists(username))
				return;
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
		}

		private void AddContactGroup()
		{
			using (var scope = new TransactionScope())
			{
				var groupOwner = Client.ContactGroupOwner;
				var group = groupOwner.AddContactGroup(ContactGroupType.General, true);
				group.Save();
				this.ContactGroup = group;
			}
		}

		public virtual void UpdateContacts(Contact[] displayedContacts, Contact[] deletedContacts)
		{
			if (this.ContactGroup == null)
				AddContactGroup();
			ContactGroup.UpdateContacts(displayedContacts, deletedContacts);
		}

		public virtual void UpdateContacts(Contact[] displayedContacts)
		{
			UpdateContacts(displayedContacts, null);
		}

        public virtual bool IsRegisteredInBilling
        {
            get
            {
                return Convert.ToUInt32(ArHelper.WithSession(session =>
                    session.CreateSQLQuery(@"
SELECT
	COUNT(*)
FROM
	Billing.Accounting
WHERE
	AccountId = :AccountId and Type = :Type")
                        .SetParameter("AccountId", Id)
                        .SetParameter("Type", AccountingItemType.Address)
                        .UniqueResult())) > 0;
            }
        }

        public virtual void RegisterInBilling()
        {
            using (var scope = new TransactionScope())
            {
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
                var accountingItem = new AccountingItem {
                    AccountId = Id,
                    Type = AccountingItemType.Address,
					Operator = SecurityContext.Administrator.UserName,
                };
                accountingItem.Create();
                scope.VoteCommit();
            }
        }

        public virtual void UnregisterInBilling()
        {
            using (var scope = new TransactionScope())
            {
				DbLogHelper.SetupParametersForTriggerLogging<User>(SecurityContext.Administrator.UserName,
					HttpContext.Current.Request.UserHostAddress);
                var accountingItem = AccountingItem.GetByAddress(this);
                accountingItem.Delete();
                scope.VoteCommit();
            }
        }

		public virtual void MoveToAnotherClient(Client newOwner)
		{
			using (var scope = new TransactionScope(OnDispose.Rollback))
			{
				MoveAddressIntersection(newOwner.Id);
				Client = newOwner;
				Update();
				scope.VoteCommit();
			}
		}

		private void MoveAddressIntersection(uint newClientId)
		{
			ArHelper.WithSession(session => session.CreateSQLQuery(@"
DROP TEMPORARY TABLE IF EXISTS Future.TempAddressIntersection;
CREATE TEMPORARY TABLE Future.TempAddressIntersection 
SELECT
	i.Id AS OldIntersectionId,
	newi.Id as NewIntersectionId,
	ai.SupplierDeliveryId,
	ai.ControlMinReq,
	ai.MinReq
FROM
	Future.Intersection i
	JOIN Future.addressintersection ai on i.Id = ai.IntersectionId
	JOIN Future.Intersection newi on newi.ClientId = :NewClientId and newi.RegionId = i.regionId and newi.PriceId = i.priceid
WHERE i.ClientId = :OldClientId
;

INSERT INTO Future.AddressIntersection(AddressId,IntersectionId,SupplierDeliveryId,ControlMinReq,MinReq)
(SELECT
	:AddressId, tmp.NewIntersectionId, tmp.SupplierDeliveryId, tmp.ControlMinReq, tmp.MinReq 
FROM
	Future.TempAddressIntersection AS tmp);

DELETE
FROM Future.AddressIntersection 
WHERE 
	AddressId = :AddressId AND 
	IntersectionId IN (SELECT OldIntersectionId FROM Future.TempAddressIntersection);

DROP TEMPORARY TABLE IF EXISTS Future.TempAddressIntersection;
")
				.SetParameter("AddressId", Id)
				.SetParameter("NewClientId", newClientId)
				.SetParameter("OldClientId", Client.Id)
				.ExecuteUpdate());
		}
	}
}
