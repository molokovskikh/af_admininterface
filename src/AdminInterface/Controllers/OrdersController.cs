using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Mail;
using System.Text;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Properties;
using AdminInterface.Queries;
using Common.Tools;
using NHibernate.Criterion;
using System.Linq;
using Common.Web.Ui.NHibernateExtentions;
using RemoteOrderSenderService;

namespace AdminInterface.Controllers
{
	public class OrdersController : AdminInterfaceController
	{
		public void Details(uint id)
		{
			var order = DbSession.Load<ClientOrder>(id);
			PropertyBag["order"] = order;
			PropertyBag["lines"] = order.Lines(DbSession);
			CancelLayout();
		}

		public void Make(uint supplierId)
		{
			var supplier = DbSession.Get<Supplier>(supplierId);
			PropertyBag["supplier"] = supplier;
			var orders = DbSession.CreateSQLQuery(@"
Select oh.RowId
FROM orders.ordershead oh
	join usersettings.pricesdata pd on pd.pricecode = oh.pricecode
	join orders.OrdersList ol on ol.OrderId = oh.rowId
where pd.FirmCode = :SupplierId
group by oh.RowId
having count(ol.RowId) > 0
limit 5").SetParameter("SupplierId", supplier.Id).List<uint>();
			PropertyBag["orders"] = orders.Implode();
			PropertyBag["Formaters"] = OrderHandler.Formaters();
			PropertyBag["thisFormat"] = supplier.OrderRules.Count > 0 ? supplier.OrderRules.First().Formater.ClassName : string.Empty;
		}

		public void DoMakeOrders(string orders, string email, string formater)
		{
			var isGood = true;
			var mailMessage = new MailMessage { Subject = "Сформированные заказы" };
			mailMessage.To.Add(email);
			mailMessage.From = new MailAddress("tech@analit.net", "Сервис отправки заказов", Encoding.UTF8);
			if (!string.IsNullOrEmpty(orders))
				foreach (var order in orders.Split(',')) {
					var orderProc = new RemoteOrderSendServiceHelper(Settings.Default.WCFOrderSenderServiceUrl);
					FileData[] result = null;
#if !DEBUG
					result = orderProc.CreateOrder(Convert.ToUInt32(order), formater);
#endif
					if (result != null)
						foreach (var attachment in result.Select(t => new Attachment(t.Stream, t.FileName))) {
							mailMessage.Attachments.Add(attachment);
						}
					else {
						Error("Не удалось получить все файлы от сервиса, некоторые заказы не были сформированы, проверте почту");
						isGood = false;
					}
				}
			else {
				Error("Вы не указали номера заказов");
				isGood = false;
			}
			var smtpClient = new SmtpClient { Host = Func.GetSmtpServer() };
			smtpClient.Send(mailMessage);
			if (isGood)
				Notify("Заказы сформированы и отправлены, проверте почту");
			RedirectToReferrer();
		}
	}
}