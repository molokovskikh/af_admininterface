using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models.Certificates
{
	[ActiveRecord(Schema = "documents", Lazy = true), Auditable]
	public class CertificateSource : ActiveRecordLinqBase<CertificateSource>
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo(Column = "FtpSupplierId")]
		public virtual Supplier Supplier { get; set; }

		[Property]
		public virtual string SourceClassName { get; set; }

		[Property]
		public virtual DateTime FtpFileDate { get; set; }

		[Property]
		public virtual bool SearchInAssortmentPrice { get; set; }

		[Property(Column = "PersonOrientationName")]
		public virtual string Name { get; set; }

		[HasAndBelongsToMany(typeof (Supplier),
			Lazy = true,
			ColumnKey = "CertificateSourceId",
			Table = "SourceSuppliers",
			Schema = "Documents",
			ColumnRef = "SupplierId",
			Cascade = ManyRelationCascadeEnum.SaveUpdate)]
		public virtual IList<Supplier> Suppliers { get; set; }

		public virtual string GetName()
		{
			return string.IsNullOrEmpty(Name) ? SourceClassName : Name;
		}
	}
}