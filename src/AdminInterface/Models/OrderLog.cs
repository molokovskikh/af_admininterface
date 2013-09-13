using System;
using System.Collections.Generic;
using AdminInterface.Security;
using Common.Tools;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.NHibernateExtentions;

namespace AdminInterface.Models
{
	public class OrderLog
	{
		public bool IsOldOrderForQueue
		{
			get { return WriteTime.AddMinutes(5) < DateTime.Now; }
		}
		public uint Id { get; set; }
		public uint? ClientOrderId { get; set; }
		public DateTime WriteTime { get; set; }
		public DateTime PriceDate { get; set; }

		public uint AddressId { get; set; }
		public uint UserId { get; set; }
		public uint SupplierId { get; set; }

		public string Drugstore { get; set; }
		public string Address { get; set; }
		public string User { get; set; }

		public string Supplier { get; set; }
		public string PriceName { get; set; }

		public uint PriceId { get; set; }

		public uint RowCount { get; set; }
		public double Sum { get; set; }
		public uint? ResultCode { get; set; }
		public uint? TransportType { get; set; }

		public uint SmtpId { get; set; }

		public string Region { get; set; }

		public bool Submited { get; set; }

		public bool Deleted { get; set; }

		public string Addition { get; set; }

		public string GetResult()
		{
			if(Deleted)
				return "Удален";
			if(!Submited)
				return "Не подтвержден";
			if (TransportType == null || ResultCode == 0)
				return "Не отправлен";

			if (PriceId == 2647)
				return "ok (Обезличенный заказ)";

			if (Addition != null && Addition.IndexOf("Отказ:", StringComparison.CurrentCultureIgnoreCase) >= 0)
				return Addition;

			switch (TransportType) {
				case 1:
					return ResultCode.ToString();
				case 2:
					return "ok (Ftp Инфорум)";
				case 4:
					return "ok (Ftp Поставщика)";
				default:
					return "ok (Собственный отправщик)";
			}
		}
	}
}