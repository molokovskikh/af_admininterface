using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Certificates;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Models.Telephony;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(HttpUtility)),
		Secure(PermissionType.ViewSuppliers),
	]
	public class SuppliersController : AdminInterfaceController
	{
		public void Show(uint id)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			PropertyBag["supplier"] = supplier;
			PropertyBag["users"] = supplier.Users;
			PropertyBag["contactGroups"] = supplier.ContactGroupOwner.ContactGroups;
			PropertyBag["usersInfo"] = ADHelper.GetPartialUsersInformation(supplier.Users);

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["messages"] = ClientInfoLogEntity.MessagesForClient(supplier);
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl;

			Sort.Make(this);
			var hast = supplier.GetHashCode();
			if (IsPost)
			{
				BindObjectInstance(supplier, "supplier");
				if (IsValid(supplier))
				{
					supplier.Save();
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
		}

		public void ChangePayer(uint supplierId, uint payerId)
		{
			var suplier = Supplier.Find(supplierId);

			SecurityContext.Administrator.CheckRegion(suplier.HomeRegion.Id);
			if (!SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
				throw new NotHavePermissionException();

			var payer = Payer.Find(payerId);
			suplier.ChangePayer(payer);

			Notify("Изменено");
			RedirectToAction("Show", new {id = suplier.Id});
		}

		public void SendMessage(uint id, string message)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			if (!string.IsNullOrWhiteSpace(message))
			{
				new ClientInfoLogEntity(message, supplier).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		[ManualAuditable]
		public void ChangeSertificateSource(uint supplierId, uint sertificateSourceId)
		{
			var supplier = Supplier.Find(supplierId);
			var oldSource = CertificateSource.Queryable.Where(c => c.Suppliers.Contains(supplier)).ToList();
			var logMessage = new StringBuilder();
			oldSource.ForEach(s => {
				s.Suppliers.Remove(supplier);
				logMessage.AppendLine(string.Format("Удален источник сертификатов {0}", s.GetName()));
				s.Save();
			});
			if (sertificateSourceId > 0) { 
				var sertSource = CertificateSource.Find(sertificateSourceId);
				sertSource.Suppliers.Add(supplier);
				logMessage.AppendLine(string.Format("Установлен источник сертификатов {0}", sertSource.GetName()));
				sertSource.Save();
			}
			Notify("Сохранено");

			new ClientInfoLogEntity(logMessage.ToString(), supplier).Save();

			RedirectToReferrer();
		}

		[return : JSONReturnBinder]
		public object[] GetCertificateSourses()
		{
			Func<CertificateSource, string> predicate = c => !string.IsNullOrEmpty(c.Name) ? c.Name : c.SourceClassName;
			return CertificateSource.Queryable.ToList()
				.OrderBy(predicate)
				.Select(c => new {c.Id, Name = predicate(c)})
				.ToArray();
		}
	}
}
