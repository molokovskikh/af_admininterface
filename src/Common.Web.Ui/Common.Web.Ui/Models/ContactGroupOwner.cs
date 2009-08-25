using System;
using Castle.ActiveRecord;
using System.Collections.Generic;
using Common.Web.Ui.Helpers;

namespace Common.Web.Ui.Models
{
	[ActiveRecord("contacts.contact_group_owners")]
	public class ContactGroupOwner : ActiveRecordBase<ContactGroupOwner>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[HasMany(ColumnKey = "ContactGroupOwnerId", Inverse = true, Lazy = true)]
		public virtual IList<ContactGroup> ContactGroups { get; set; }

		public ContactGroup AddContactGroup(ContactGroupType type)
		{
			var group = new ContactGroup(type, BindingHelper.GetDescription(type));
			group.ContactGroupOwner = this;
			ContactGroups.Add(group);
			return group;
		}
	}
}
