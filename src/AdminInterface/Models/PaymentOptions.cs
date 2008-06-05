using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace AdminInterface.Models
{
	public class PaymentOptions
	{
		public DateTime? BeginOfPayPeriod { get; set; }
		public string Comment { get; set; }
		public bool ClientServForFree { get; set; }

		public string GetCommentAddion()
		{
			return "";
		}
	}
}
