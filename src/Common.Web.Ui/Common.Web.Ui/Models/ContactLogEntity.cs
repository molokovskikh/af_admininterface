using System;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;

namespace Common.Web.Ui.Models
{
	public enum OperationType
	{
		Add = 1,
		Delete = 2,
		Update = 3
	}

	[ActiveRecord("ContactLogs", Schema = "logs")]
	public class ContactLogEntity : ActiveRecordBase<ContactLogEntity>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public uint ContactGroupOwnerId { get; set; }

		[Property]
		public string ContactText { get; set; }

		[Property]
		public string ContactGroupName { get; set; }

		[Property]
		public OperationType OperationType { get; set; }

		[Property]
		public string Host { get; set; }

		[Property]
		public string UserName { get; set; }

		[Property]
		public DateTime LogTime { get; set; }

		public ContactLogEntity()
		{
			LogTime = DateTime.Now;
		}

		public ContactLogEntity(Contact contact, string userName, string host, OperationType operationType)
		{
			LogTime = DateTime.Now;
			UserName = userName;
			Host = host;
			OperationType = operationType;
			ContactText = contact.ContactText;
			ContactGroup contactGroup;
			if (contact.ContactOwner is Person)
				contactGroup = ((Person)contact.ContactOwner).ContactGroup;
			else
				contactGroup = ((ContactGroup)contact.ContactOwner);
			ContactGroupOwnerId = contactGroup.ContactGroupOwner.Id;
			ContactGroupName = contactGroup.Name;
		}
	}
}
