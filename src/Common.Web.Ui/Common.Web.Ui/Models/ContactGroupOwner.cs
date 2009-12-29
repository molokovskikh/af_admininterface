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

		[HasMany(ColumnKey = "ContactGroupOwnerId", Inverse = true, Lazy = true, Cascade = ManyRelationCascadeEnum.SaveUpdate)]
		public virtual IList<ContactGroup> ContactGroups { get; set; }

		public virtual ContactGroup AddContactGroup(ContactGroupType type)
		{
			return AddContactGroup(type, false);
		}

		public virtual ContactGroup AddContactGroup(ContactGroupType type, bool specialized)
		{
			if (ContactGroups == null)
				ContactGroups = new List<ContactGroup>();

			var group = new ContactGroup(type, BindingHelper.GetDescription(type));
			group.Specialized = specialized;
			group.ContactGroupOwner = this;
			ContactGroups.Add(group);
			return group;			
		}
	}
}
