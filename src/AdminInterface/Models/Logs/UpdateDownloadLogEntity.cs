using System;
using System.Net;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Table = "UpdateDownloadLogs", Schema = "logs")]
	public class UpdateDownloadLogEntity
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
			try
			{
				return Dns.GetHostEntry(ClientHost).HostName;
			}
			catch
			{
				return "-";
			}
		}
	}
}