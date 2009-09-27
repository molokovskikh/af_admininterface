﻿using System.Collections.Generic;
using System.Linq;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;

namespace AdminInterface.Models
{
	[ActiveRecord(Table = "UserSettings.Defaults")]
	public class DefaultValues : ActiveRecordBase<DefaultValues>
	{
		[PrimaryKey]
		public uint Id { get; set; }

		[Property]
		public uint AnalitFVersion { get; set; }

		[BelongsTo("FormaterId")]
		public OrderHandler Formater { get; set; }

		[BelongsTo("SenderId")]
		public OrderHandler Sender{ get; set; }

		public static DefaultValues Get()
		{
			return FindAll().First();
		}
	}

	public enum HandlerType
	{
		Formater = 1,
		Sender = 2,
	}

	[ActiveRecord(Table = "OrderSendRules.order_handlers")]
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
