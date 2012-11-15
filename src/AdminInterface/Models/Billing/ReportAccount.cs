using AdminInterface.Helpers;
using AdminInterface.Models.Logs;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models.Audit;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(DiscriminatorValue = "2")]
	public class ReportAccount : Account
	{
		public ReportAccount()
		{
		}

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

		public override bool Enabled
		{
			get { return Status; }
		}

		[Style]
		public bool Disabled
		{
			get { return !Status; }
		}

		public override bool Status
		{
			get { return Report.Allow; }
			set
			{
				Report.Allow = value;
				Report.ChangeComment = Comment;
			}
		}

		public override string DefaultDescription
		{
			get
			{
				if (Payer.Recipient != null)
					return Payer.Recipient.ReportDescription;
				return "";
			}
		}
	}
}