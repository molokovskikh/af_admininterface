
using System.Web.Mvc;
using AdminInterface.Models.Security;
using AdminInterface.Security;

namespace AdminInterface.Controllers.MVC
{
	[Secure]
	public class BaseMvController : Common.Web.Ui.Helpers.MvcController
	{
	}
}