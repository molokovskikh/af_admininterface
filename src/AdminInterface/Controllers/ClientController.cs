using AdminInterface.Filters;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	public class ClientController : SmartDispatcherController
	{
		public void Info(uint cc)
		{
			Client client = Client.Find(cc);
			PropertyBag["Client"] = client;
			PropertyBag["Admin"] = Session["Admin"];
			PropertyBag["ContactGroups"] = client.ContactGroupOwner.ContactGroups;
		}
	}
}
