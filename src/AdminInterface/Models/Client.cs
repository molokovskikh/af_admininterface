using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Criterion;

namespace AdminInterface.Models
{
	public enum ClientStatus
	{
		[Description("��������")] Off = 0,
		[Description("�������")] On = 1,
	}

	public enum ClientType
	{
		[Description("���������")] Supplier = 0,
		[Description("������")] Drugstore = 1,
	}

	public enum Segment
	{
		[Description("���")] Wholesale = 0,
		[Description("�������")] Retail = 1,
	}

	[ActiveRecord("usersettings.clientsdata", Lazy = true)]
	public class Client : ActiveRecordBase<Client>
	{
		[PrimaryKey("FirmCode")]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string ShortName { get; set; }

		[Property]
		public virtual string FullName { get; set; }

		[Property("Adress")]
		public virtual string Address { get; set; }

		[Property("FirmStatus")]
		public virtual ClientStatus Status { get; set; }

		[Property]
		public virtual ClientStatus BillingStatus { get; set; }

		[Property("FirmType")]
		public virtual ClientType Type { get; set; }

		[Property("FirmSegment")]
		public virtual Segment Segment { get; set; }

		[Property]
		public virtual DateTime RegistrationDate { get; set; }

		[Property]
		public virtual string Registrant { get; set; }

		[Property]
		public virtual UInt64 MaskRegion { get; set; }

		[BelongsTo("ContactGroupOwnerId")]
		public virtual ContactGroupOwner ContactGroupOwner { get; set; }

		[BelongsTo("BillingCode")]
		public virtual Payer BillingInstance { get; set; }

		[BelongsTo("RegionCode")]
		public virtual Region HomeRegion { get; set; }

		[HasMany(ColumnKey = "ClientCode", Inverse = true, Lazy = true, OrderBy = "OsUserName")]
		public virtual IList<User> Users { get; set; }

		[HasMany(typeof(Relationship), Inverse = true, Lazy = true, ColumnKey = "IncludeClientCode", Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Relationship> Parents { get; set; }

		[HasMany(typeof(Relationship), Inverse = true, Lazy = true, ColumnKey = "PrimaryClientCode")]
		public virtual IList<Relationship> Children { get; set; }

		[HasMany(typeof(ShowRelationship), Inverse = true, Lazy = true, ColumnKey = "ShowClientCode", Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<ShowRelationship> ShowClients { get; set; }

		public virtual IEnumerable<User> GetUsers()
		{
			foreach (var user in Users)
				yield return user;

			if (Type == ClientType.Drugstore)
				yield break;

			foreach (var client in ShowClients)
				foreach (var user in client.Parent.Users)
					yield return user;	
		}

		public virtual bool IsDrugstore()
		{
			return Type == ClientType.Drugstore;
		}

		public virtual bool IsClientActive()
		{
			return Status == ClientStatus.On && BillingStatus == ClientStatus.On;
		}

		public virtual float GetPayment(IList<Tariff> tariffs)
		{
			RelationshipType? includeType = null;
			if (Parents.Count > 0)
			    includeType = Parents[0].RelationshipType;

			var tariff = tariffs.FirstOrDefault(t => t.Region.Id == HomeRegion.Id
			                                         && t.IncludeType == includeType);

			if (tariff == null)
				return 0;

			return tariff.Pay;
		}

		public static Client FindAndCheck(uint clientCode)
		{
			var client = Find(clientCode);

			SecurityContext.Administrator.CheckClientHomeRegion(client.HomeRegion.Id);
			SecurityContext.Administrator.CheckClientType(client.Type);
			return client;
		}

		public static Client FindClietnForBilling(uint clientCode)
		{
			return ArHelper.WithSession(
				session => session.CreateCriteria(typeof (Client))
				           	.Add(Expression.Eq("Id", clientCode))
				           	.SetFetchMode("BillingInstance", FetchMode.Join)
				           	//.SetFetchMode("Payer.Clients", FetchMode.Eager)
				           	.SetFetchMode("HomeRegion", FetchMode.Join)
				           	.SetFetchMode("ContactGroupOwner", FetchMode.Join)
				           	//.SetFetchMode("ContactGroupOwner.ContactGroups", FetchMode.Eager)
				           	//.SetFetchMode("ContactGroupOwner.ContactGroups.Contacts", FetchMode.Eager)
				           	//.SetFetchMode("ContactGroupOwner.ContactGroups.Persons", FetchMode.Eager)
				           	.UniqueResult<Client>());
		}

		public virtual string GetAddressForSendingClientCard()
		{
			if (Type == ClientType.Drugstore)
				return Build(GetContactGroup(ContactGroupType.General),
				             GetContactGroup(ContactGroupType.OrderManagers));

			return Build(GetContactGroup(ContactGroupType.General),
			             GetContactGroup(ContactGroupType.OrderManagers),
			             GetContactGroup(ContactGroupType.ClientManagers));
		}

		public virtual ContactGroup GetContactGroup(ContactGroupType type)
		{
			foreach (var contactGroup in ContactGroupOwner.ContactGroups)
				if (contactGroup.Type == type)
					return contactGroup;
			return null;
		}

		public virtual void ProcessEmails(List<string> emails, params ContactOwner[] contactGroups)
		{
			foreach (var contactGroup in contactGroups)
				foreach (var contact in contactGroup.Contacts)
					if (contact.Type == ContactType.Email && !emails.Contains(contact.ContactText.Trim()))
						emails.Add(contact.ContactText.Trim());
		}

		private string Build(ContactGroup generalGroup, params ContactGroup[] specialGroup)
		{
			var emails = new List<string>();
			foreach (var contactGroup in specialGroup)
			{
				if (contactGroup.Persons != null)
				{
					foreach (var person in contactGroup.Persons)
						ProcessEmails(emails, person);
				}
			}

			ProcessEmails(emails, specialGroup);

			if (emails.Count > 0)
				return String.Join(", ", emails.ToArray());

			if (generalGroup.Persons != null)
			{
				foreach (var person in generalGroup.Persons)
					ProcessEmails(emails, person);
			}

			ProcessEmails(emails, generalGroup);
			
			return String.Join(", ", emails.ToArray());
		}

		public virtual string GetSubordinateType()
		{
			if (Parents.Count == 0)
				return "-";

			return BindingHelper.GetDescription(Parents[0].RelationshipType);
		}

		public virtual string GetNameWithParents()
		{
			if (Parents.Count == 0)
				return ShortName;

			return ShortName + "[" + Parents[0].Parent.ShortName + "]";
		}

		public virtual string GetIdWithParentId()
		{
			if (Parents.Count == 0)
				return Id.ToString();

			return Id + "[" + Parents[0].Parent.Id + "]";
		}

		public virtual void ResetUin()
		{
			ArHelper.WithSession<Client>(session =>
			                             session
			                             	.CreateSQLQuery(@"
update usersettings.UserUpdateInfo uui
	join usersettings.OsUserAccessRight ouar on uui.UserId = ouar.RowId
set uui.AFCopyId = '' 
where ouar.clientcode = :clientcode")
			                             	.SetParameter("clientcode", Id)
			                             	.ExecuteUpdate());
		}

		public virtual bool HaveUin()
		{
			var result = ArHelper.WithSession(session =>
			                                  session
			                                  	.CreateSQLQuery(@"
select sum(length(concat(uui.AFCopyId))) = 0
from usersettings.UserUpdateInfo uui
	join usersettings.OsUserAccessRight ouar on uui.UserId = ouar.RowId
where ouar.clientcode = :clientcode
group by ouar.clientcode")
			                                  	.SetParameter("clientcode", Id)
			                                  	.UniqueResult<long?>());

			return result != null && result.Value == 0;
		}

		public virtual bool CanChangeStatus()
		{
			if (Type != ClientType.Drugstore)
				return true;

			if (BillingStatus == ClientStatus.Off || Status == ClientStatus.Off)
				return true;

			if (Children.Count == 0)
				return true;

			foreach (var child in Children)
			{
				if (child.Child.Status == ClientStatus.On && child.Child.BillingStatus == ClientStatus.On)
					return false;
			}

			return true;
		}

		public virtual bool HavePreparedData()
		{
			foreach (var user in GetUsers())
			{
				var file = String.Format(@"U:\wwwroot\ios\Results\{0}.zip", user.Id);
				if (File.Exists(file))
					return true;
			}
			return false;
		}

		public virtual Relationship AddRelationship(Client parent, RelationshipType type)
		{
			var relationship = new Relationship(parent, this, type);
			Parents.Add(relationship);
			if (type == RelationshipType.Base || type == RelationshipType.BasePlus)
				ShowClients.Add(new ShowRelationship(parent, this));
			return relationship;
		}

		public virtual void RemoveRelationship(Relationship relationship)
		{
			Parents.Remove(relationship);
			var showRelationship = ShowClients.FirstOrDefault(s => s.Parent.Id == relationship.Parent.Id);
			if (showRelationship != null)
			{
				ShowClients.Remove(showRelationship);
				showRelationship.Delete();
			}

			relationship.Delete();
		}
	}
}