using System;
using System.Linq;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.MonoRailExtentions;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	public class RevisionActsController : SmartDispatcherController
	{
		public void Show(uint id, DateTime? begin, DateTime? end)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				PropertyBag["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			PropertyBag["beginDate"] = begin.Value;
			PropertyBag["endDate"] = end.Value;
			PropertyBag["act"] = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Act.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());
			PropertyBag["payer"] = payer;
		}

		public void Print(uint id, DateTime? begin, DateTime? end)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Flash["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				RedirectToReferrer();
				return;
			}
			LayoutName = "Print";

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			PropertyBag["act"] = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Act.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());
			PropertyBag["payer"] = payer;
		}

		public void Mail(uint id, DateTime? begin, DateTime? end, string emails)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Flash["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			var act = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Act.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());

			this.Mail().RevisionAct(act, emails).Send();

			Flash["Message"] = Message.Notify("Отправлено");
			RedirectToReferrer();
		}

		public void Excel(uint id, DateTime? begin, DateTime? end)
		{
			var payer = Payer.Find(id);
			if (payer.Recipient == null)
			{
				Flash["Message"] = Message.Error("У плательщика не указан получатель платежей, выберете получаетля платежей.");
				RedirectToReferrer();
				return;
			}

			if (begin == null)
				begin = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			if (end == null)
				end = DateTime.Now;

			var act = new RevisionAct(payer,
				begin.Value,
				end.Value,
				Act.Queryable.Where(i => i.Payer == payer).ToList(),
				Payment.Queryable.Where(p => p.Payer == payer).ToList());

			Exporter.ToResponse(Response, Exporter.Export(act));
			CancelView();
		}
	}
}