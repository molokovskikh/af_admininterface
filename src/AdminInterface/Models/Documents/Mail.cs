﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AddUser;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Documents
{
	public enum RecipientType
	{
		Address = 0,
		Region = 1,
		Client = 2
	}

	[ActiveRecord(Schema = "Documents")]
	public class Mail
	{
		public Mail()
		{
			Attachments = new List<Attachment>();
			Recipients = new List<MailRecipient>();
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual DateTime LogTime { get; set; }

		[Property]
		public virtual string SupplierEmail { get; set; }

		[Property]
		public virtual string Subject { get; set; }

		[Property]
		public virtual string Body { get; set; }

		[BelongsTo("SupplierId")]
		public virtual Supplier Supplier { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<Attachment> Attachments { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<MailRecipient> Recipients { get; set; }

		public virtual void AddRecipient(Client client)
		{
			Recipients.Add(new MailRecipient {
				Mail = this,
				Client = client,
				Type = RecipientType.Client
			});
		}
	}

	[ActiveRecord(Schema = "Logs")]
	public class MailSendLog
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[BelongsTo("MailId")]
		public virtual Mail Mail { get; set; }

		[BelongsTo("UpdateId")]
		public virtual UpdateLogEntity Update { get; set; }

		[BelongsTo("RecipientId")]
		public virtual MailRecipient Recipient { get; set; }
	}

	[ActiveRecord(Schema = "Documents")]
	public class MailRecipient
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual RecipientType Type { get; set; }

		[BelongsTo("MailId")]
		public virtual Mail Mail { get; set; }

		[BelongsTo("RegionId")]
		public virtual Region Region { get; set; }

		[BelongsTo("ClientId")]
		public virtual Client Client { get; set; }

		[BelongsTo("AddressId")]
		public virtual Address Address { get; set; }

		public override string ToString()
		{
			if (Region != null)
				return String.Format("Регион " + Region.Name);
			if (Address != null)
				return String.Format("Адрес доставки " + Address.Value);
			if (Client != null)
				return String.Format("Клиент " + Client.Name);
			return "";
		}
	}

	[ActiveRecord(Schema = "Documents")]
	public class Attachment
	{
		public Attachment()
		{
			SendLogs = new List<AttachmentSendLog>();
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("MailId")]
		public virtual Mail Mail { get; set; }

		[Property]
		public virtual string Filename { get; set; }

		[Property]
		public virtual string Extension { get; set; }

		[Property]
		public virtual uint Size { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.All)]
		public virtual IList<AttachmentSendLog> SendLogs { get; set; }

		public virtual string FullFilename
		{
			get { return Filename + Extension; }
		}

		public string StorageFilename(AppConfig config)
		{
			return Path.Combine(config.AttachmentsPath, Id + Extension);
		}

		public UpdateLogEntity GetAttachmentLog(User user)
		{
			var log = SendLogs.FirstOrDefault(l => l.User == user);
			if (log == null)
				return null;
			return log.Update;
		}
	}

	[ActiveRecord(Schema = "Logs")]
	public class AttachmentSendLog
	{
		public AttachmentSendLog()
		{}

		public AttachmentSendLog(User user, Attachment attachment, UpdateLogEntity update)
		{
			User = user;
			Attachment = attachment;
			Update = update;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UserId")]
		public virtual User User { get; set; }

		[BelongsTo("AttachmentId")]
		public virtual Attachment Attachment { get; set; }

		[BelongsTo("UpdateId")]
		public UpdateLogEntity Update { get; set; }
	}
}