using System;
using System.Linq;
using Castle.ActiveRecord;
using System.Collections.Generic;
using Common.Web.Ui.Helpers;

namespace Common.Web.Ui.Models
{
	[ActiveRecord("contact_group_owners", Schema = "contacts")]
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

		public bool HaveGroup(ContactGroupType type)
		{
			return ContactGroups.Any(g => g.Type == type);
		}
	}
}
