using System;
using System.Linq;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class RevisionActsController : AdminInterfaceController
	{
		public void Show(uint id, DateTime? begin, DateTime? end)
		{
			var payer = DbSession.Load<Payer>(id);
			if (payer.Recipient == null) {
				Error("У плательщика не указан получатель платежей, выберете получателя платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, 1, 1);
			if (end == null)
				end = DateTime.Now;

			PropertyBag["beginDate"] = begin.Value;
			PropertyBag["endDate"] = end.Value;
			PropertyBag["act"] = BuildRevisionAct(begin.Value, end.Value, payer);
			PropertyBag["payer"] = payer;
		}

		public void Print(uint id, DateTime? begin, DateTime? end)
		{
			var payer = DbSession.Load<Payer>(id);
			if (payer.Recipient == null) {
				Error("У плательщика не указан получатель платежей, выберете получателя платежей.");
				RedirectToReferrer();
				return;
			}

			LayoutName = "Print";
			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			PropertyBag["act"] = BuildRevisionAct(begin.Value, end.Value, payer);
			PropertyBag["payer"] = payer;
		}

		public void Mail(uint id, DateTime? begin, DateTime? end, string emails, string message)
		{
			var payer = DbSession.Load<Payer>(id);
			if (payer.Recipient == null) {
				Error("У плательщика не указан получатель платежей, выберете получателя платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			var act = BuildRevisionAct(begin.Value, end.Value, payer);
			Mail().RevisionAct(act, emails, message);

			Notify("Отправлено");
			RedirectToReferrer();
		}

		public void Excel(uint id, DateTime? begin, DateTime? end)
		{
			var payer = DbSession.Load<Payer>(id);
			if (payer.Recipient == null) {
				Error("У плательщика не указан получатель платежей, выберете получателя платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			var act = BuildRevisionAct(begin.Value, end.Value, payer);

			Exporter.ToResponse(Response, Exporter.Export(act));
			CancelView();
		}

		private RevisionAct BuildRevisionAct(DateTime begin, DateTime end, Payer payer)
		{
			return new RevisionAct(payer,
				begin,
				end,
				DbSession.Query<Act>().Where(i => i.Payer == payer).ToList(),
				DbSession.Query<Payment>().Where(p => p.Payer == payer).ToList(),
				DbSession.Query<BalanceOperation>().Where(o => o.Payer == payer).ToList());
		}
	}
}