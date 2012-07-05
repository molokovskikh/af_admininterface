﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using AdminInterface.Models.Validators;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.ActiveRecord.Linq;
using Castle.Components.Validator;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models
{
	public enum HandlerType
	{
		Formater = 1,
		Sender = 2,
	}

	[ActiveRecord(Table = "Defaults", Schema = "UserSettings")]
	public class DefaultValues
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

		[Property, ValidateNonEmpty, Description("Разрешенные расширения вложений мини-почты"), ExtensionListValidation]
		public string AllowedMiniMailExtensions { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseSubjectMiniMailOnUnknownProvider { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseBodyMiniMailOnUnknownProvider { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseSubjectMiniMailOnEmptyRecipients { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseBodyMiniMailOnEmptyRecipients { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseSubjectMiniMailOnMaxAttachment { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseBodyMiniMailOnMaxAttachment { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseSubjectMiniMailOnAllowedExtensions { get; set; }

		[Property, ValidateNonEmpty]
		public string ResponseBodyMiniMailOnAllowedExtensions { get; set; }

		public IEnumerable<string> GetPhones()
		{
			return Phones.Split(new [] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);
		}

		public string AppendFooter(string body)
		{
			return body + "\r\n\r\n" + EmailFooter;
		}
	}

	[ActiveRecord(Table = "order_send_rules", Schema = "OrderSendRules"), Auditable]
	public class OrderSendRules : ActiveRecordBase<OrderHandler>, IAuditable
	{
		public OrderSendRules()
		{
			ErrorNotificationDelay = 60;
		}

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

		[BelongsTo("FormaterId"), Auditable("Форматер")]
		public OrderHandler Formater { get; set; }

		[BelongsTo("SenderId"), Auditable("Отправщик")]
		public OrderHandler Sender{ get; set; }

		[Property]
		public ulong? RegionCode { get; set; }

		[Property]
		public bool SendDebugMessage { get; set; }

		[Property]
		public uint ErrorNotificationDelay { get; set; }

		public IAuditRecord GetAuditRecord()
		{
			return new ClientInfoLogEntity(Supplier);
		}
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

		public virtual string Name
		{
			get { return ClassName; }
		}

		public static IList<OrderHandler> Senders()
		{
			return Queryable.Where(h => h.Type == HandlerType.Sender).OrderBy(h => h.ClassName).ToList();
		}

		public static IList<OrderHandler> Formaters()
		{
			return Queryable.Where(h => h.Type == HandlerType.Formater).OrderBy(h => h.ClassName).ToList();
		}

		public static IList<OrderHandler> All()
		{
			return Queryable.OrderBy(h => h.ClassName).ToList();
		}

		public override string ToString()
		{
			return ClassName;
		}
	}
}
