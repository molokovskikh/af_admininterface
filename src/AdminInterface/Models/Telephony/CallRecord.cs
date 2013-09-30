using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Telephony
{
	public enum CallType
	{
		[Description("Исходящий")] Outgoing = 0,
		[Description("Входящий")] Incoming = 1,
		[Description("Отзвон")] Callback = 2,
	}

	[ActiveRecord("RecordCalls", Schema = "logs")]
	public class CallRecord
	{
		private IList<CallRecordFile> _files = null;

		[PrimaryKey]
		public virtual ulong Id { get; set; }

		[Property]
		public virtual string From { get; set; }

		[Property]
		public virtual string To { get; set; }

		[Property]
		public virtual DateTime WriteTime { get; set; }

		[Property(Column = "NameFrom")]
		public virtual string NameSource { get; set; }

		[Property(Column = "NameTo")]
		public virtual string NameDestination { get; set; }

		[Property(Column = "CallType")]
		public virtual CallType? Type { get; set; }

		public virtual string GetCallType()
		{
			if (Type == null)
				return "Неизвестно";
			return Type.GetDescription();
		}

		public virtual IList<CallRecordFile> Files
		{
			get
			{
				if (_files == null) {
					_files = new List<CallRecordFile>();
					var searchPattern = String.Format("{0}*", Id);
					var files = Directory.GetFiles(ConfigurationManager.AppSettings["CallRecordsDirectory"], searchPattern);
					foreach (var file in files)
						_files.Add(new CallRecordFile(file));
				}
				return _files;
			}
		}
	}
}