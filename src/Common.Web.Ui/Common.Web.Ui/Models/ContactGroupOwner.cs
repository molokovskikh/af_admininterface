using Castle.ActiveRecord;
using System.Collections.Generic;

namespace Common.Web.Ui.Models
{
	[ActiveRecord("contacts.contact_group_owners")]
	public class ContactGroupOwner : ActiveRecordBase<ContactGroupOwner>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[HasMany(ColumnKey = "ContactGroupOwnerId", Inverse = true, Lazy = true)]
		public virtual IList<ContactGroup> ContactGroups { get; set; }
	}
}
