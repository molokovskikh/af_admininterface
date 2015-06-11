using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web.UI;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Helpers;
using System.Linq;
using Castle.ActiveRecord;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;
using DataBinder = Castle.Components.Binder.DataBinder;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
	]
	public class MailForSupplierController : AdminInterfaceController
	{
		[AccessibleThrough(Verb.Get)]
		public void SendMailForNewSupplier()
		{
			NewSupplierMessage message = new NewSupplierMessage();
			message.DownLoad(Response);
			CancelView();
		}
	}
}