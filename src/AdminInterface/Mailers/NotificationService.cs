using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Helpers;
using AdminInterface.Mailers;
using AdminInterface.Models;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;
using AdminInterface.Properties;
using NHibernate;

namespace AdminInterface.Services
{
	public class NotificationService
	{
		private readonly string _messageTemplateForSupplierAboutDrugstoreRegistration =
			@"Добрый день.

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, зарегистрирован новый клиент: {0} ( {1} ) по адресу {2} в регионе(городе) {3}.
Пожалуйста произведите настройки для данного клиента (Раздел 'Личный кабинет' на сайте www.analit.net ).

Адрес доставки накладных: {4}@waybills.analit.net
Адрес доставки отказов: {4}@refused.analit.net
"
				.Replace('\'', '\"');

		private readonly string _messageTemplateForSupplierAfterAddressRegistration =
			@"Добрый день.

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, для клиента: {0} ( {1} ) в регионе(городе) {2} зарегистрирован новый адрес доставки {3}.
Пожалуйста при необходимости произведите настройку кодов доставки (Раздел 'Личный кабинет' на сайте www.analit.net ).

Адрес доставки накладных: {4}@waybills.analit.net
Адрес доставки отказов: {4}@refused.analit.net
"
				.Replace('\'', '\"');

		private DefaultValues defaults;
		private ISession session;

		public NotificationService(ISession session, DefaultValues defaults)
		{
			this.defaults = defaults;
			this.session = session;
		}

		public void NotifySupplierAboutAddressRegistration(Address address)
		{
			if (!address.Client.ShouldSendNotification()
				|| !address.Enabled)
				return;

			var client = address.Client;
			var emails = GetEmailsForNotification(client);
			var orgName = client.FullName;
			if (client.Payers.Count > 1 || address.Payer.JuridicalOrganizations.Count > 1)
				orgName = address.LegalEntity.FullName;

			foreach (var email in emails)
				Func.Mail("tech@analit.net",
					"Аналитическая Компания Инфорум",
					"Новый адрес доставки в системе \"АналитФАРМАЦИЯ\"",
					defaults.AppendFooter(String.Format(_messageTemplateForSupplierAfterAddressRegistration,
						orgName,
						client.Name,
						client.HomeRegion.Name,
						address.Value,
						address.Id)),
					email,
					"",
					null);
		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client, bool isRenotify)
		{
			if (!client.ShouldSendNotification())
				return;

			var emails = GetEmailsForNotification(client);
			NotifySupplierAboutDrugstoreRegistration(client, emails);
			// Если это повторная рассылка уведомлений о регистрации, то отсылаем письмо
			// "Разослано повторное уведомление о регистрации"
			if (isRenotify)
				Mailer.ClientRegistrationResened(client);
		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client, List<string> emails)
		{
			foreach (var address in client.Addresses.Where(a => a.Enabled)) {
				foreach (var email in emails) {
					Func.Mail("tech@analit.net",
						"Аналитическая Компания Инфорум",
						"Новый клиент в системе \"АналитФАРМАЦИЯ\"",
						defaults.AppendFooter(String.Format(_messageTemplateForSupplierAboutDrugstoreRegistration,
							client.FullName,
							client.Name,
							address.Value,
							client.HomeRegion.Name,
							address.Id)),
						email,
						"",
						null);
				}
			}
		}

		public List<string> GetEmailsForNotification(Client client)
		{
			var dataAdapter = new MySqlDataAdapter(@"
select
distinct c.contactText
from Customers.Suppliers s
join contacts.contact_groups cg on s.ContactGroupOwnerId = cg.ContactGroupOwnerId
join contacts.contacts c on cg.Id = c.ContactOwnerId
join usersettings.PricesData pd on pd.FirmCode = s.Id
join usersettings.pricesregionaldata prd on prd.PriceCode = pd.PriceCode
join usersettings.PricesCosts pc on pc.PriceCode = pd.pricecode
join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
join farm.FormRules f on f.Id = pi.FormRuleId
join Customers.Intersection i on i.PriceId = pd.PriceCode and i.ClientId = ?clientId and i.RegionId = prd.RegionCode
where length(c.contactText) > 0
and s.Disabled = 0
and s.RegionMask & ?Region > 0
and cg.Type = ?ContactGroupType
and c.Type = ?ContactType
and prd.enabled = 1
and (to_seconds(now()) - to_seconds(pi.PriceDate)) < (f.maxold * 86400)
and pd.AgencyEnabled = 1
and pd.Enabled = 1
and pd.PriceType <> 1
and prd.RegionCode = ?Region
and i.AgencyEnabled = 1

union

select
distinct c.contactText
from Customers.Suppliers s
join contacts.contact_groups cg on s.ContactGroupOwnerId = cg.ContactGroupOwnerId
join contacts.persons p on cg.id = p.ContactGroupId
join contacts.contacts c on p.Id = c.ContactOwnerId
join usersettings.PricesData pd on pd.FirmCode = s.Id
join usersettings.pricesregionaldata prd on prd.PriceCode = pd.PriceCode
join usersettings.PricesCosts pc on pc.PriceCode = pd.pricecode
join usersettings.PriceItems pi on pi.Id = pc.PriceItemId
join farm.FormRules f on f.Id = pi.FormRuleId
join Customers.Intersection i on i.PriceId = pd.PriceCode and i.ClientId = ?clientId and i.RegionId = prd.RegionCode
where length(c.contactText) > 0
and s.Disabled = 0
and s.RegionMask & ?Region > 0
and cg.Type = ?ContactGroupType
and c.Type = ?ContactType
and prd.enabled = 1
and (to_seconds(now()) - to_seconds(pi.PriceDate)) < (f.maxold * 86400)
and pd.AgencyEnabled = 1
and pd.Enabled = 1
and pd.PriceType <> 1
and prd.RegionCode = ?Region
and i.AgencyEnabled = 1
;", (MySqlConnection)session.Connection);
			var parameters = dataAdapter.SelectCommand.Parameters;
			parameters.AddWithValue("?Region", client.HomeRegion.Id);
			parameters.AddWithValue("?ContactGroupType", ContactGroupType.ClientManagers);
			parameters.AddWithValue("?ContactType", ContactType.Email);
			parameters.AddWithValue("?ClientId", client.Id);
			var data = new DataSet();
			dataAdapter.Fill(data);
			return data.Tables[0].Rows.Cast<DataRow>().Select(r => r["ContactText"].ToString()).ToList();
		}
	}
}