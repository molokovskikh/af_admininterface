using AdminInterface.Filters;
using AdminInterface.Helpers;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;

namespace AdminInterface.Controllers
{
	[Filter(ExecuteEnum.BeforeAction, typeof(AuthorizeFilter)), Helper(typeof(ADHelper))]
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
