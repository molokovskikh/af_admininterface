using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AdminInterface.Models.Documents;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.MonoRailExtentions;
using AdminInterface.Queries;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	[Helper(typeof(TableHelper), "tableHelper")]
	[Helper(typeof(PaginatorHelper), "paginator")]
	[Secure(PermissionType.MiniMailModering)]
	public class MailsModeringController : AdminInterfaceController
	{
		public void ShowMails()
		{
			var filter = BindFilter<MiniMailFilter, BaseItemForTable>();
			FindFilter(filter);
		}

		public void GetMail(uint mailId)
		{
			var mail = DbSession.Get<Mail>(mailId);
			PropertyBag["mail"] = mail;
			PropertyBag["recipients"] = mail.Recipients.GroupBy(g => g.Type).Select(r => new { r.Key, items = r.ToList() });
			CancelLayout();
		}

		public void Attachment(uint id)
		{
			var attachment = DbSession.Get<Attachment>(id);
			this.RenderFile(attachment.StorageFilename(Config), attachment.Filename);
		}

		public void DeleteGroup(uint[] ids)
		{
			foreach (var item in ids) {
				Delete(item);
			}
		}

		public void Delete(uint id)
		{
			var mail = DbSession.Get<Mail>(id);
			mail.Deleted = true;
			DbSession.Save(mail);
			foreach (var mailSendLog in mail.Logs) {
				if (!mailSendLog.Committed)
					DbSession.Delete(mailSendLog);
			}
			this.Mailer().DeleteMiniMailToSupplier(mail, Defaults.DeletingMiniMailText).Send();
			this.Mailer().DeleteMiniMailToOffice(mail, Request.UserHostAddress).Send();
			Notify("Удалено");
			CancelView();
			CancelLayout();
		}

		[return: JSONReturnBinder]
		public object GetSullierList(string term)
		{
			uint id = 0;
			uint.TryParse(term, out id);
			return DbSession.Query<Supplier>().Where(c =>
				(c.Name.Contains(term) || c.Id == id))
				.ToList()
				.Select(c => new { id = c.Id, label = c.Name })
				.ToList();
		}
	}
}