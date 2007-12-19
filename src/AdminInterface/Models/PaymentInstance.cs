using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Castle.ActiveRecord;

namespace AdminInterface.Model
{
	[ActiveRecord(Table = "billing.payers")]
	public class PaymentInstance : ActiveRecordBase<PaymentInstance>
	{
		private uint _id;
		private DateTime _payDate;
		private uint _paySum;

		[PrimaryKey(Column = "payerId")]
		public uint Id
		{
			get { return _id; }
			set { _id = value; }
		}

		[Property(Column = "oldpaydate")]
		public DateTime PayDate
		{
			get { return _payDate; }
			set { _payDate = value; }
		}

		[Property(Column = "oldtariff")]
		public uint PaySum
		{
			get { return _paySum; }
			set { _paySum = value; }
		}
	}
}
