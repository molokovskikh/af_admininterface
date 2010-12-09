using System;
using System.Collections.Generic;
using System.Linq;
using AdminInterface.Controllers;
using AdminInterface.Helpers;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Linq;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Models.Billing
{
	[ActiveRecord(Schema = "billing")]
	public class Invoice : ActiveRecordLinqBase<Invoice>
	{
		public Invoice()
		{}

		public Invoice(Payer payer, Period period)
		{
			Recipient = payer.JuridicalOrganizations.First(j => j.Recipient != null).Recipient;
			Payer = payer;
			Period = period;
			Sum = payer.TotalSum;
			Date = DateTime.Now;
		}

		[PrimaryKey]
		public uint Id { get; set; }
		[BelongsTo]
		public Recipient Recipient { get; set; }
		[BelongsTo(Cascade = CascadeEnum.SaveUpdate)]
		public Payer Payer { get; set; }
		[Property]
		public decimal Sum { get; set; }
		[Property]
		public DateTime Date { get; set; }
		[Property]
		public Period Period {get; set; }


		public List<InvoicePart> Bills
		{
			get
			{
				return new List<InvoicePart> {
					new InvoicePart {
						Name = String.Format("Мониторинг оптового фармрынка за {0}", BindingHelper.GetDescription(Period)),
						Total = Sum
					}
				};
			}
		}

		public void Send(Controller controller)
		{
			if (Payer.InvoiceSettings.PrintInvoice)
			{
				new Printer().Print(controller.Context.Services.ViewEngineManager, this);
			}
			if (Payer.InvoiceSettings.EmailInvoice)
			{
				var mailer = new MonorailMailer();
				mailer.Invoice(this);
				mailer.Send();
			}
			Payer.Balance -= Sum;
		}
	}

	public class InvoicePart
	{
		public string Name { get; set; }
		public decimal Total { get; set; }
	}
}