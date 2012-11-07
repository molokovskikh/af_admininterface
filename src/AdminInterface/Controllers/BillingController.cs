using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
using AdminInterface.Models;
using AdminInterface.Models.Billing;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using AdminInterface.Models.Suppliers;
using AdminInterface.Security;
using AdminInterface.MonoRailExtentions;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Castle.Components.Validator;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.Models;
using Common.Web.Ui.MonoRailExtentions;
using NHibernate.Linq;

namespace AdminInterface.Controllers
{
	public class BillingFilter
	{
		private string _tab;

		public string Tab
		{
			get { return _tab; }
			set
			{
				if (String.IsNullOrEmpty(value))
					return;

				_tab = value;
			}
		}

		public uint PayerId { get; set; }
		public uint ServiceId { get; set; }
		public string ActiveTab { get; set; }

		public IList<LogMessageType> Types { get; set; }

		private Service service;
		private Payer payer;

		public BillingFilter()
		{
			Tab = "payments";
			Types = new List<LogMessageType> { LogMessageType.User };
		}


		public bool IsSystem
		{
			get { return Types.Contains(LogMessageType.System); }
		}

		public Payer Payer
		{
			get
			{
				if (payer == null)
					payer = Payer.Find(PayerId);
				return payer;
			}
		}

		public Service Service
		{
			get
			{
				if (service == null && ServiceId != 0)
					service = ActiveRecordMediator<Service>.FindByPrimaryKey(ServiceId);
				return service;
			}
		}

		public string FilterRow(dynamic log)
		{
			var result = new StringBuilder();
			var filterValue = (string)(log.LogType.ToString() + log.ObjectId.ToString());
			result.AppendFormat("data-filter=\"{0}\" ", filterValue);
			if (IsFiltred) {
				var currentFilterValue = GetFilterValue(Service);

				if (filterValue.ToLower() != currentFilterValue.ToLower())
					result.AppendFormat("style=\"display:none\"");
			}
			return result.ToString();
		}

		public string GetFilterValue(Service service)
		{
			var currentFilterValue = service.Id.ToString();
			if (service is Client)
				currentFilterValue = "Client" + currentFilterValue;
			else
				currentFilterValue = "Supplier" + currentFilterValue;
			return currentFilterValue;
		}

		private bool IsFiltred
		{
			get { return Service != null; }
		}

		public Dictionary<string, string> Parts()
		{
			var result = new Dictionary<string, string> {
				{ "BillingCode", PayerId.ToString() }
			};
			if (ServiceId != 0)
				result.Add("ClientCode", ServiceId.ToString());
			if (!String.IsNullOrEmpty(ActiveTab))
				result.Add("Tab", ActiveTab);
			return result;
		}
	}

	[
		Helper(typeof(BindingHelper)),
		Helper(typeof(ViewHelper)),
		Secure(PermissionType.Billing),
	]
	public class BillingController : AdminInterfaceController
	{
		public BillingController()
		{
			SetBinder(new ARDataBinder());
		}

		public void Edit(uint billingCode,
			uint clientCode,
			string tab,
			uint currentJuridicalOrganizationId,
			[SmartBinder(Expect = "filter.Types")] BillingFilter filter)
		{
			filter.ServiceId = clientCode;
			filter.PayerId = billingCode;
			filter.Tab = tab;

			var payer = filter.Payer;

			var userIds = payer.Users.Select(u => u.Id).ToArray();
			PropertyBag["UsersMessages"] = ActiveRecordLinqBase<UserMessage>.Queryable
				.Where(m => userIds.Contains(m.Id) && m.ShowMessageCount > 0)
				.ToList();

			PropertyBag["filter"] = filter;
			PropertyBag["LogRecords"] = AuditLogRecord.GetLogs(payer, filter.Types.Contains(LogMessageType.System));
			PropertyBag["Instance"] = payer;
			PropertyBag["payer"] = payer;
			PropertyBag["MailSentHistory"] = MailSentEntity.GetHistory(payer);
			PropertyBag["Today"] = DateTime.Today;
			PropertyBag["Recipients"] = DbSession.Query<Recipient>().OrderBy(r => r.Name).ToList();
			PropertyBag["suppliers"] = DbSession.Query<Supplier>().Where(s => s.Payer == payer).OrderBy(s => s.Name).ToList();
			PropertyBag["clients"] = payer.Clients.OrderBy(c => c.Name).ToList();
			PropertyBag["Users"] = payer.Users.Where(u => u.RootService.Type != ServiceType.Supplier).ToList();
			PropertyBag["Addresses"] = payer.Addresses;
			PropertyBag["Reports"] = payer.GetReportAccounts();
			PropertyBag["userMessage"] = new UserMessage();

			if (currentJuridicalOrganizationId > 0)
				PropertyBag["currentJuridicalOrganizationId"] = currentJuridicalOrganizationId;
		}

		public void SendMessage(uint BillingCode, string messageText)
		{
			var payer = DbSession.Get<Payer>(BillingCode);
			var message = new PayerAuditRecord(payer, messageText);
			DbSession.Save(message);
			RedirectToReferrer();
		}

		public void Update(
			[ARDataBind("Instance", AutoLoadBehavior.NullIfInvalidKey)] Payer payer,
			uint clientCode,
			string tab)
		{
			payer.CheckCommentChangesAndLog(this.Mailer());
			payer.Update();
			Notify("Изменения сохранены");
			RedirectToReferrer();
		}

		public void SendMessage([DataBind("userMessage")] UserMessage message,
			uint clientCode,
			uint billingCode,
			string tab)
		{
			try {
				if (message.Id != 0) {
					var user = DbSession.Load<User>(message.Id);
					SendMessageToUser(user, message);
				}
				else {
					var payer = Payer.Find(billingCode);
					foreach (var user in payer.Users)
						SendMessageToUser(user, message);
				}
				if (message.Mail)
					Mailer.SendMessageFromBillingToClient(message);
				Notify("Сообщение сохранено");
			}
			catch (ValidationException exception) {
				Flash["SendError"] = exception.ValidationErrorMessages[0];
			}

			RedirectToReferrer();
		}

		private void SendMessageToUser(User user, UserMessage clientMessage)
		{
			var message = UserMessage.FindUserMessage(user.Id);
			if (message == null)
				return;
			message.Message = clientMessage.Message;
			message.ShowMessageCount = clientMessage.ShowMessageCount;
			message.Update();

			DbSession.Save(new UserMessageSendLog(message));
		}

		public void UpdateClientStatus(uint id, bool status, string addComment)
		{
			var service = ActiveRecordMediator<Service>.FindByPrimaryKey(id);
			var oldDisabled = service.Disabled;
			service.Disabled = !status;
			service.EditComment = addComment;
			ActiveRecordMediator<Service>.Save(service);
			DbSession.Flush();
			if (oldDisabled != service.Disabled) {
				this.Mailer().EnableChanged(service, addComment).Send();
			}
			CancelView();
		}

		public void Search()
		{
			var filter = new PayerFilter();
			if (IsPost || Request.QueryString.Keys.Cast<string>().Any(k => k.StartsWith("filter."))) {
				((ARDataBinder)Binder).AutoLoad = AutoLoadBehavior.NullIfInvalidKey;
				BindObjectInstance(filter, IsPost ? ParamStore.Form : ParamStore.QueryString, "filter");
				PropertyBag["searchResults"] = filter.Find();
			}
			PropertyBag["filter"] = filter;
		}

		public void SentMail(uint clientCode, string tab, [DataBind("MailSentEntity")] MailSentEntity sentEntity)
		{
			try {
				sentEntity.UserName = Admin.UserName;
				sentEntity.SaveAndFlush();
				Notify("Cохранено");
			}
			catch (ValidationException ex) {
				Flash["SendMailError"] = ex.ValidationErrorMessages[0];
			}
			Redirect("Billing", "Edit", new { clientCode, billingCode = sentEntity.PayerId, tab });
		}

		public void DeleteMail(uint id)
		{
			var mailSend = MailSentEntity.Find(id);
			mailSend.Delete();
			CancelView();
		}

		public void ShowMessageForUser(uint userId)
		{
			CancelLayout();
			var message = UserMessage.FindUserMessage(userId);
			PropertyBag["Message"] = message;
			PropertyBag["user"] = DbSession.Load<User>(message.Id);
		}

		public void CancelMessage(uint userId)
		{
			var message = UserMessage.Find(userId);
			message.ShowMessageCount = 0;
			message.Update();
			CancelView();
		}

		public void AdditionalSupplierInfo(uint supplierId, string cssClassName)
		{
			CancelLayout();
			var supplier = DbSession.Load<Supplier>(supplierId);
			PropertyBag["supplier"] = supplier;
			PropertyBag["PricesRegions"] = supplier.PricesRegions;
		}

		public void AdditionalUserInfo(uint userId, string cssClassName)
		{
			CancelLayout();
			var user = DbSession.Load<User>(userId);
			PropertyBag["user"] = user;
			PropertyBag["regions"] = Region.All().Where(r => (r.Id & user.WorkRegionMask) > 0).ToArray();
		}

		public void AdditionalAddressInfo(uint addressId, string cssClassName)
		{
			CancelLayout();
			PropertyBag["address"] = Address.Find(addressId);
		}

		public void SearchUsersForAddress(uint addressId, string searchText)
		{
			if (String.IsNullOrEmpty(searchText))
				searchText = String.Empty;
			var address = Address.Find(addressId);
			var users = address.Client.Users.Where(user =>
				((!String.IsNullOrEmpty(user.Name) && user.Name.ToLower().Contains(searchText.ToLower())) || (user.Login.ToLower().Contains(searchText.ToLower()))) &&
					(!address.AvaliableFor(user)));
			PropertyBag["Users"] = users;
			CancelLayout();
		}

		public void SearchAddressesForUser(uint userId, string searchText)
		{
			if (String.IsNullOrEmpty(searchText))
				searchText = String.Empty;
			var user = DbSession.Load<User>(userId);
			var addresses = user.Client.Addresses.Where(address =>
				address.Value.ToLower().Contains(searchText.ToLower()) &&
					!address.AvaliableFor(user));
			PropertyBag["Addresses"] = addresses;
			CancelLayout();
		}

		public void ConnectUserToAddress(uint userId, uint addressId)
		{
			var user = DbSession.Load<User>(userId);
			var address = Address.Find(addressId);
			address.AvaliableForUsers.Add(user);
			DbSession.SaveOrUpdate(address.Client);

			CancelView();
		}

		public void DisconnectUserFromAddress(uint userId, uint addressId)
		{
			var user = DbSession.Load<User>(userId);
			var address = Address.Find(addressId);
			var client = user.Client;

			address.AvaliableForUsers.Remove(user);
			DbSession.SaveOrUpdate(client);

			CancelView();
		}

		[return: JSONReturnBinder]
		public string TotalSum(uint payerId)
		{
			return Payer.Find(payerId).PaymentSum.ToString("C");
		}

		public void JuridicalOrganizations(uint payerId, uint currentJuridicalOrganizationId)
		{
			var payer = Payer.Find(payerId);
			PropertyBag["Payer"] = payer;
			PropertyBag["tab"] = "juridicalOrganization";
			PropertyBag["Addresses"] = payer.Addresses;
			if (currentJuridicalOrganizationId > 0)
				PropertyBag["currentJuridicalOrganization"] = LegalEntity.Find(currentJuridicalOrganizationId);
		}

		public void UpdateJuridicalOrganizationInfo([ARDataBind("juridicalOrganization", AutoLoad = AutoLoadBehavior.NullIfInvalidKey)] LegalEntity juridicalOrganization)
		{
			var organization = LegalEntity.Find(juridicalOrganization.Id);
			organization.Name = juridicalOrganization.Name;
			organization.FullName = juridicalOrganization.FullName;
			organization.Update();

			Notify("Сохранено");
			RedirectToReferrer();
		}

		public void AddJuridicalOrganization([ARDataBind("juridicalOrganization", AutoLoad = AutoLoadBehavior.NewRootInstanceIfInvalidKey)] LegalEntity legalEntity, uint payerId)
		{
			var payer = Payer.Find(payerId);
			legalEntity.Payer = payer;
			legalEntity.CreateAndFlush();
			Maintainer.LegalEntityCreated(legalEntity);

			Notify("Юридическое лицо создано");
			RedirectToReferrer();
		}
	}
}