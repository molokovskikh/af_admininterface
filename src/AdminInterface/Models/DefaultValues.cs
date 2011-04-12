using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Models.Suppliers;
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

		[Property]
		public string Phones { get; set; }

		[BelongsTo("FormaterId")]
		public OrderHandler Formater { get; set; }

		[BelongsTo("SenderId")]
		public OrderHandler Sender{ get; set; }

		public static DefaultValues Get()
		{
			return FindAll().First();
		}

		public IEnumerable<string> GetPhones()
		{
			return Phones.Split(new [] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
		}

		public string AppendFooter(string body)
		{
			return body + "\r\n\r\n" + EmailFooter;
		}
	}

	[ActiveRecord(Table = "order_send_rules", Schema = "OrderSendRules")]
	public class OrderSendRules : ActiveRecordBase<OrderHandler>
	{
		public OrderSendRules()
		{}

		public OrderSendRules(DefaultValues defaults, Supplier supplier)
		{
			Formater = defaults.Formater;
			Sender = defaults.Sender;
			Supplier = supplier;
		}

		[PrimaryKey]
		public uint Id { get; set; }

		[BelongsTo("FirmCode")]
		public Supplier Supplier { get; set; }

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
