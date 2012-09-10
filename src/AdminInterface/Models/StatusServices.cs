using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminInterface.Models
{
	public class StatusServices
	{
		private string _orderProcStatus;
		private string _priceProcessorMasterStatus;

		public StatusServices()
		{
		}

		public StatusServices(string OrderProcStatus, string PriceProcessorMasterStatus)
		{
			_orderProcStatus = OrderProcStatus;
			_priceProcessorMasterStatus = PriceProcessorMasterStatus;
		}

		public virtual string OrderProcStatus
		{
			get { return _orderProcStatus; }
			set { _orderProcStatus = value; }
		}

		public virtual string PriceProcessorMasterStatus
		{
			get { return _priceProcessorMasterStatus; }
			set { _priceProcessorMasterStatus = value; }
		}

		public virtual bool OrderProcNotRunnigOrUnknown
		{
			get { return ((_orderProcStatus == "Недоступна") || (_orderProcStatus == "Не запущена")); }
		}

		public virtual bool PriceProcessorMasterNotRunnigOrUnknown
		{
			get { return ((_priceProcessorMasterStatus == "Недоступна") || (_priceProcessorMasterStatus == "Не запущена")); }
		}
	}
}