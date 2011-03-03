using System;
using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models
{
	public enum HandlerType
	{
		Formater = 1,
		Sender = 2,
	}

	[ActiveRecord(Table = "Defaults", Schema = "UserSettings")]
	public class DefaultValues : ActiveRecordBase<DefaultValues>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public uint AnalitFVersion { get; set; }

		[Property]
		public string EmailFooter { get; set; }

		[BelongsTo("FormaterId")]
		public OrderHandler Formater { get; set; }

		[BelongsTo("SenderId")]
		public OrderHandler Sender{ get; set; }

		public static DefaultValues Get()
		{
			return FindAll().First();
		}

		public string AppendFooter(string format)
		{
			throw new NotImplementedException();
		}
	}

	[ActiveRecord(Table = "order_send_rules", Schema = "OrderSendRules")]
	public class OrderSendRules : ActiveRecordBase<OrderHandler>
	{
		public OrderSendRules()
		{}

		public OrderSendRules(DefaultValues defaults, uint firmCode)
		{
			Formater = defaults.Formater;
			Sender = defaults.Sender;
			FirmCode = firmCode;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public uint FirmCode { get; set; }

		[BelongsTo("FormaterId")]
		public OrderHandler Formater { get; set; }

		[BelongsTo("SenderId")]
		public OrderHandler Sender{ get; set; }
	}

	[ActiveRecord(Table = "order_handlers", Schema = "OrderSendRules")]
	public class OrderHandler : ActiveRecordLinqBase<OrderHandler>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public string ClassName { get; set; }

		[Property]
		public HandlerType Type { get; set; }

		public static IList<OrderHandler> GetSenders()
		{
			return Queryable.Where(h => h.Type == HandlerType.Sender).OrderBy(h => h.ClassName).ToList();
		}

		public static IList<OrderHandler> GetFormaters()
		{
			return Queryable.Where(h => h.Type == HandlerType.Formater).OrderBy(h => h.ClassName).ToList();
		}
	}
}
