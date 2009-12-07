using System;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using AdminInterface.Services;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Layout("billing"), 
		Helper(typeof(BindingHelper)), 
		Helper(typeof(ViewHelper)),
		Secure(PermissionType.RegisterDrugstore, PermissionType.RegisterSupplier, Required = Required.AnyOf)
	]
	public class RegisterController : SmartDispatcherController
	{
		private readonly NotificationService _notificationService = new NotificationService();

		public void Register(uint id, uint clientCode, bool showRegistrationCard)
		{
			var instance = Payer.Find(id);
			PropertyBag["Instance"] = instance;
			PropertyBag["showRegistrationCard"] = showRegistrationCard;
			PropertyBag["clientCode"] = clientCode;
			PropertyBag["PaymentOptions"] = new PaymentOptions();
			PropertyBag["admin"] = SecurityContext.Administrator;
		}

		public void Registered([ARDataBind("Instance", AutoLoadBehavior.Always)] Payer payer,
			[DataBind("PaymentOptions")] PaymentOptions paymentOptions,
			uint clientCode,
			bool showRegistrationCard)
		{
			if (String.IsNullOrEmpty(payer.Comment))
				payer.Comment = paymentOptions.GetCommentForPayer();
			else
				payer.Comment += "\r\n" + paymentOptions.GetCommentForPayer();

			payer.UpdateAndFlush();

			var client = Client.Find(clientCode);

			_notificationService.SendNotificationToBillingAboutClientRegistration(client,
				SecurityContext.Administrator.UserName,
				paymentOptions, NotificationHelper.GetApplicationUrl());

			if (showRegistrationCard)
				RedirectToUrl("../report.aspx");
			else
				RedirectToUrl(String.Format("../client/{0}", clientCode));
		}
	}
}
