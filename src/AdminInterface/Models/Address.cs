using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord("future.address")]
	public class Address : ActiveRecordBase<Address>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property("Address")]
		public string Value { get; set; }

		[BelongsTo("ClientCode")]
		public Client Client { get; set; }

		[BelongsTo("DocumentResiverId")]
		public User DocumentsResiver { get; set; }

		[HasAndBelongsToMany(typeof (User),
			Lazy = true,
			ColumnKey = "AddressId",
			Table = "future.UserAddress",
			ColumnRef = "UserId")]
		public virtual IList<User> AvaliableForUsers { get; set; }

		public virtual bool AvaliableFor(User user)
		{
			return AvaliableForUsers.Any(u => u.Id == user.Id);
		}
	}
}