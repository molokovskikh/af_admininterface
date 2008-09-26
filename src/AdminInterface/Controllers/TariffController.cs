using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models.Billing;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(ViewHelper)),
		Helper(typeof(BindingHelper))
	]
	public class TariffController : SmartDispatcherController
	{
		public void Show()
		{
			PropertyBag["Tariffs"] = Tariff.FindAll();
		}

		public void Update(Tariff[] tariffs)
		{

		}
	}
}
