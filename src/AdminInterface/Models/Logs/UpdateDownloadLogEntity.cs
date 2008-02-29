using System;
using System.Net;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "logs.UpdateDownloadLogs")]
	public class UpdateDownloadLogEntity : ActiveRecordBase<AnalitFDownloadLogEntity>
	{
		[PrimaryKey]
		public int Id { get; set; }

		[Property]
		public string ClientHost { get; set; }

		[Property]
		public DateTime LogTime { get; set; }

		[Property]
		public ulong FromByte { get; set; }

		[Property]
		public ulong SendBytes { get; set; }

		[Property]
		public ulong TotalBytes { get; set; }

		[BelongsTo("UpdateId")]
		public UpdateLogEntity UpdateLog { get; set; }

		public string ResolveHost()
		{
			var host = Dns.GetHostEntry(ClientHost);
			if (host == null)
				return "-";
			return host.HostName;
		}
	}
}