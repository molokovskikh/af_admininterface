using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	public class ClientController : SmartDispatcherController
	{
		public void Info(uint cc)
		{
			Client client = Client.Find(cc);
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
		}
	}
}
