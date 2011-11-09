using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "2")]
	public class ReportAccount : Account
	{
		public ReportAccount() {}

		public ReportAccount(Report report)
		{
			Report = report;
			_readyForAccounting = true;
		}

		[BelongsTo("ObjectId", Cascade = CascadeEnum.All)]
		public virtual Report Report { get; set; }

		public override Payer Payer
		{
			get { return Report.Payer; }
		}

		public override string Name
		{
			get { return Report.Comment; }
		}

		public override LogObjectType ObjectType
		{
			get { return LogObjectType.Report; }
		}

		public override uint ObjectId
		{
			get { return Report.Id; }
		}

		public override bool ShouldPay()
		{
			return Report.Allow && base.ShouldPay();
		}

		[Style]
		public bool Disabled
		{
			get { return !Status; }
		}

		public override bool Status
		{
			get
			{
				return Report.Allow;
			}
			set
			{
				Report.Allow = value;
			}
		}

		public override string DefaultDescription
		{
			get
			{
				return "Статистический отчет по фармрынку за {0}";
			}
		}
	}
}