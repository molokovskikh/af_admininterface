using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Initializers;
using Castle.ActiveRecord;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Models.AFNet
{
	[ActiveRecord(Schema = "Customers")]
	public class BinChannel
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }

		[Property]
		public virtual string Version { get; set; }

		[Property]
		public virtual string Dir { get; set; }

		public static KeyValuePair<string, string>[] Load(ISession session, User user)
		{
			var items = session.Query<BinChannel>()
				.ToArray()
				.Select(x => new KeyValuePair<string, string>(x.Dir, $"{x.Name} ({x.Version})"))
				.ToArray();
			//не нужно сбрасывать значение если оно отсутствует в справочнике
			if (user.AFNetConfig.BinUpdateChannel != null
					&& !items.Any(x => x.Key == user.AFNetConfig.BinUpdateChannel)) {
				return new[] { new KeyValuePair<string, string>(user.AFNetConfig.BinUpdateChannel, user.AFNetConfig.BinUpdateChannel), }
					.Concat(items).ToArray();
			}
			return items;
		}
	}
}