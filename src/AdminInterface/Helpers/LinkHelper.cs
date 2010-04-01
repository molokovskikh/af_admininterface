using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminInterface.Helpers
{
	public class LinkHelper
	{
		public static string Address(string siteroot, uint addressId, string text)
		{
			var url = String.Format("<a href=\"{0}/deliveries/{1}/edit\">{2}</a>", siteroot, addressId, text);
			return url;
		}

		public static string User(string siteroot, string login, string text)
		{
			var url = String.Format("<a href=\"{0}/users/{1}/edit\">{2}</a>", siteroot, login, text);
			return url;
		}
	}
}
