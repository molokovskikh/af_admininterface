using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Models.Logs
{
	[ActiveRecord(Schema = "Logs", Table = "CertificateRequestLogs", SchemaAction = "none")]
	public class CertificateRequestLog
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo("UpdateId")]
		public virtual UpdateLogEntity Update { get; set; }

		[BelongsTo("DocumentBodyId")]
		public virtual DocumentLine Line { get; set; }
	}
}