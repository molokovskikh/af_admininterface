using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AdminInterface.Controllers.MVC;
using AdminInterface.Models;
using AdminInterface.ViewModels;
using AdminInterface.ViewModels.Reports;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class VManagerReportsController : BaseMvController
	{
		// GET: VManagerReports
		public ActionResult WaybillStatistics(uint clientId ,  ReportTable<WaybillStatisticsFilter, WaybillStatisticsData> model)
		{
			model.TableHead.Headers = new[] { "Поставщик", "Аптека", "Количество заявок на данного поставщика", " Количество накладных", "Количество разобранных накладных" };
			model.TableFilter.ClientId = clientId;
			model.TablePaginator.TotalItems = WaybillStatisticsData.GetReportTotalItemsNumber(DbSession, model);
			model.TableData = WaybillStatisticsData.GetReportData(DbSession, model);

			//классический интерфейс
			ViewBag.ClassicStyle = true;
			return View(model);
		}
	}
}