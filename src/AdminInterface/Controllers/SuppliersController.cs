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
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.ActiveRecord;
using Castle.Components.Binder;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Tools;
using Common.Web.Ui.Controllers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.Models.Audit;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;
using Newtonsoft.Json;

namespace AdminInterface.Controllers
{
	[
		Helper(typeof(HttpUtility)),
		Secure(PermissionType.ViewSuppliers),
	]
	public class SuppliersController : AdminInterfaceController
	{
		public void Show(uint id, [SmartBinder(Expect = "filter.Types")] MessageQuery filter)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			PropertyBag["supplier"] = supplier;
			PropertyBag["users"] = supplier.Users;

			PropertyBag["contactGroups"] = supplier.ContactGroupOwner.ContactGroups.OrderBy(s => s.Type == ContactGroupType.General ? 0 : 1);
			PropertyBag["usersInfo"] = ADHelper.GetPartialUsersInformation(supplier.Users);

			PropertyBag["CallLogs"] = UnresolvedCall.LastCalls;
			PropertyBag["CiUrl"] = Properties.Settings.Default.ClientInterfaceUrl + "auth/logon.aspx";

			PropertyBag["filter"] = filter;
			PropertyBag["messages"] = filter.Execute(supplier, DbSession);

			Sort.Make(this);
			if (IsPost) {
				BindObjectInstance(supplier, "supplier");
				if (IsValid(supplier)) {
					DbSession.SaveOrUpdate(supplier);
					Notify("Сохранено");
					RedirectToReferrer();
				}
			}
		}

		public void WaybillExcludeFiles(uint supplierId)
		{
			var supplier = DbSession.Get<Supplier>(supplierId);
			PropertyBag["supplier"] = supplier;
		}

		public void AddNewExcludeFile(uint supplierId, string newWaybillFile)
		{
			var supplier = DbSession.Get<Supplier>(supplierId);
			var newFile = new WaybillExcludeFile(newWaybillFile, supplier);
			if (IsValid(newFile)) {
				DbSession.Save(newFile);
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		public void SaveExcludeFiles([ARDataBind("files", AutoLoadBehavior.NullIfInvalidKey)]WaybillExcludeFile[] files)
		{
			foreach (var waybillExcludeFile in files.Where(f => f != null)) {
				if (IsValid(waybillExcludeFile)) {
					DbSession.Save(waybillExcludeFile);
				}
			}
			RedirectToReferrer();
		}

		[return: JSONReturnBinder]
		public uint DeleteMask(uint maskId)
		{
			var mask = DbSession.Get<WaybillExcludeFile>(maskId);
			var supplierId = mask.Supplier.Id;
			DbSession.Delete(mask);
			Notify("Удалено");
			return supplierId;
		}

		public void ChangePayer(uint supplierId, uint payerId)
		{
			var suplier = DbSession.Load<Supplier>(supplierId);

			SecurityContext.Administrator.CheckRegion(suplier.HomeRegion.Id);
			if (!SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
				throw new NotHavePermissionException();

			var payer = DbSession.Load<Payer>(payerId);
			suplier.ChangePayer(payer);

			Notify("Изменено");
			RedirectToAction("Show", new { id = suplier.Id });
		}

		public void SendMessage(uint id, string message)
		{
			var supplier = ActiveRecordMediator<Supplier>.FindByPrimaryKey(id);
			if (!string.IsNullOrWhiteSpace(message)) {
				new AuditRecord(message, supplier).Save();
				Notify("Сохранено");
			}
			RedirectToReferrer();
		}

		[ManualAuditable]
		public void ChangeSertificateSource(uint supplierId, uint sertificateSourceId)
		{
			var supplier = DbSession.Load<Supplier>(supplierId);
			var oldSource = DbSession.Query<CertificateSource>().Where(c => c.Suppliers.Contains(supplier)).ToList();
			var logMessage = new StringBuilder();
			oldSource.ForEach(s => {
				s.Suppliers.Remove(supplier);
				logMessage.AppendLine(string.Format("Удален источник сертификатов {0}", s.GetName()));
				DbSession.Save(s);
			});
			if (sertificateSourceId > 0) {
				var sertSource = DbSession.Load<CertificateSource>(sertificateSourceId);
				sertSource.Suppliers.Add(supplier);
				logMessage.AppendLine(string.Format("Установлен источник сертификатов {0}", sertSource.GetName()));
				DbSession.Save(sertSource);
			}
			Notify("Сохранено");

			new AuditRecord(logMessage.ToString(), supplier) { MessageType = LogMessageType.System }.Save();

			RedirectToReferrer();
		}

		[return: JSONReturnBinder]
		public object[] GetCertificateSourses()
		{
			Func<CertificateSource, string> predicate = c => !string.IsNullOrEmpty(c.Name) ? c.Name : c.SourceClassName;
			return DbSession.Query<CertificateSource>().ToList()
				.OrderBy(predicate)
				.Select(c => new { c.Id, Name = predicate(c) })
				.ToArray();
		}

		public void Delete(uint id)
		{
			var supplier = DbSession.Load<Supplier>(id);

			if (!supplier.CanDelete(DbSession)) {
				Error("Не могу удалить поставщика");
				RedirectToReferrer();
				return;
			}

			supplier.Delete(DbSession);
			Notify("Удалено");
			Redirect("Users", "Search");
		}

		public void WaybillSourceSettings(uint supplierId)
		{
			var supplier = DbSession.Load<Supplier>(supplierId);
			var source = supplier.WaybillSource ?? new WaybillSource(supplier);
			if (IsPost) {
				source.session = DbSession;
				Bind(source, "source");
				if (supplier.WaybillSource == null)
					supplier.WaybillSource = source;
				source.EMailFrom = source.Emails.Implode();
				if (IsValid(source)) {
					DbSession.Save(source);
					Notify("Сохранено");
					RedirectToAction("WaybillSourceSettings", new { supplierId = source.Id });
				}
				else {
					Error("Ошибка сохранения", PropertyBag);
				}
			}
			var sourceTypes = BindingHelper.GetDescriptionsDictionary(typeof(WaybillSourceType));
			sourceTypes.Remove((int)WaybillSourceType.Http);
			PropertyBag["supplier"] = supplier;
			PropertyBag["source"] = source;
			PropertyBag["sourceTypes"] = sourceTypes;
		}
	}
}
