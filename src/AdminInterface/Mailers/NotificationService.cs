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

namespace AdminInterface.Services
{
	public class NotificationService
	{
		public NotificationService(DefaultValues defaults)
		{
			this.defaults = defaults;
		}

		private readonly string _messageTemplateForSupplierAboutDrugstoreRegistration =
			@"Добрый день.

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, зарегистрирован новый клиент: {0} ( {1} ) по адресу {2} в регионе(городе) {3}.
Пожалуйста произведите настройки для данного клиента (Раздел 'Для зарегистрированных пользователей' на сайте www.analit.net ).

Адрес доставки накладных: {4}@waybills.analit.net
Адрес доставки отказов: {4}@refused.analit.net
".Replace('\'', '\"');

		private readonly string _messageTemplateForSupplierAfterAddressRegistration =
			@"Добрый день.

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, для клиента: {0} ( {1} ) в регионе(городе) {2} зарегистрирован новый адрес доставки {3}.
Пожалуйста при необходимости произведите настройку кодов доставки (Раздел 'Для зарегистрированных пользователей' на сайте www.analit.net ).

Адрес доставки накладных: {4}@waybills.analit.net
Адрес доставки отказов: {4}@refused.analit.net
".Replace('\'', '\"');

		private DefaultValues defaults;

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
			// Если это повторная рассылка уведомлений о регистрации, то отсылаем письмо
			// "Разослано повторное уведомление о регистрации"
			if (isRenotify)
				Mailer.ClientRegistrationResened(client);
		}

		private List<string> GetEmailsForNotification(Client client)
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString())) {
				connection.Open();
				var dataAdapter = new MySqlDataAdapter(@"
select c.contactText
from Customers.Suppliers s
  join contacts.contact_groups cg on s.ContactGroupOwnerId = cg.ContactGroupOwnerId
    join contacts.contacts c on cg.Id = c.ContactOwnerId
where length(c.contactText) > 0
	and s.Id in (select pd.FirmCode
		from pricesdata as pd, pricesregionaldata as prd
		where prd.enabled = 1)
	and s.Disabled = 0
	and s.RegionMask & ?Region > 0
	and cg.Type = ?ContactGroupType
	and c.Type = ?ContactType

union

select c.contactText
from Customers.Suppliers s
  join contacts.contact_groups cg on s.ContactGroupOwnerId = cg.ContactGroupOwnerId
    join contacts.persons p on cg.id = p.ContactGroupId
      join contacts.contacts c on p.Id = c.ContactOwnerId
where length(c.contactText) > 0
	and s.Id in (select pd.FirmCode
		from pricesdata as pd, pricesregionaldata as prd
		where prd.enabled = 1)
	and s.Disabled = 0
	and s.RegionMask & ?Region > 0
	and cg.Type = ?ContactGroupType
	and c.Type = ?ContactType;", connection);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?Region", client.HomeRegion.Id);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?ContactGroupType", ContactGroupType.ClientManagers);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?ContactType", ContactType.Email);
				var data = new DataSet();
				dataAdapter.Fill(data);
				return data.Tables[0].Rows.Cast<DataRow>().Select(r => r["ContactText"].ToString()).ToList();
			}
		}
	}
}