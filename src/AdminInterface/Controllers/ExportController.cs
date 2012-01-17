using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[Secure(PermissionType.ManagerReport)]
	public class ExportController : SmartDispatcherController
	{
		private void PrepareExcelHeader(string fileName)
		{
			CancelLayout();
			CancelView();

			Response.Clear();
			Response.AppendHeader("Content-Disposition", 
				String.Format("attachment; filename=\"{0}\"", Uri.EscapeDataString(fileName)));
			Response.ContentType = "application/vnd.ms-excel";
		}

		public void GetUsersAndAdresses([DataBind("filter")]UserFinderFilter userFilter)
		{
			PrepareExcelHeader("Пользовалети_и_адреса.xls");

			var buf = ExportModel.GetUserOrAdressesInformation(userFilter);

			Response.OutputStream.Write(buf, 0, buf.Length);
		}
	}
}