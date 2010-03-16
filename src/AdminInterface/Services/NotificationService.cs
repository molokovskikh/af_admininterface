using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using AdminInterface.Helpers;
using AdminInterface.Models;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;
using AdminInterface.Properties;

namespace AdminInterface.Services
{
	public class NotificationService
	{
		private readonly Action<MailMessage> _sendMessage;

		private readonly string _messageTemplateForSupplierAboutDrugstoreRegistration =
@"Добрый день.

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, зарегистрирован новый клиент: {0} ( {1} ) в регионе(городе) {2}.
Пожалуйста произведите настройки для данного клиента (Раздел 'Для зарегистрированных пользователей' на сайте www.analit.net ).

Адрес доставки накладных: {3}@waybills.analit.net
Адрес доставки отказов: {3}@refused.analit.net

С уважением, Аналитическая компания 'Инфорум', г. Воронеж
".Replace('\'', '\"') + Settings.Default.InforoomContactPhones;

		private readonly string _messageTemplateForSupplierAfterAddressRegistration =
@"Добрый день.

В информационной системе 'АналитФАРМАЦИЯ', участником которой является Ваша организация, для клиента: {0} ( {1} ) в регионе(городе) {2} зарегистрирован новый адрес доставки {3}.
Пожалуйста при необходимости произведите настройку кодов доставки (Раздел 'Для зарегистрированных пользователей' на сайте www.analit.net ).

Адрес доставки накладных: {4}@waybills.analit.net
Адрес доставки отказов: {4}@refused.analit.net

С уважением, Аналитическая компания 'Инфорум', г. Воронеж
".Replace('\'', '\"') + Settings.Default.InforoomContactPhones;

		public NotificationService(Action<MailMessage> sendMessage)
		{
			_sendMessage = sendMessage;
		}

		public NotificationService()
		{
			_sendMessage = Func.SendWitnStandartSender;
		}

		public void NotifySupplierAboutAddressRegistration(Address address)
		{
			var client = address.Client;
			var emails = GetEmailsForNotification(client);
			foreach (var email in emails)
				Func.Mail("tech@analit.net",
					"Аналитическая Компания Инфорум",
					"Новый адрес доставки в системе \"АналитФАРМАЦИЯ\"",
					String.Format(_messageTemplateForSupplierAfterAddressRegistration,
						client.FullName,
						client.Name,
						client.HomeRegion.Name,
						address.Value,
						address.Id),
					email,
					"",
					null);
			Mailer.AddressRegistrationResened(client, address.Value);
		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client)
		{
			NotifySupplierAboutDrugstoreRegistration(client, true);
		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client, bool isRenotify)
		{
			var emails = GetEmailsForNotification(client);
			foreach (var email in emails)
				Func.Mail("tech@analit.net",
					"Аналитическая Компания Инфорум",
					"Новый клиент в системе \"АналитФАРМАЦИЯ\"",
					String.Format(_messageTemplateForSupplierAboutDrugstoreRegistration,
						client.FullName,
						client.Name,
						client.HomeRegion.Name,
						client.Addresses.First().Id),
					email,
					"",
					null);
			// Если это повторная рассылка уведомлений о регистрации, то отсылаем письмо
			// "Разослано повторное уведомление о регистрации"
			if (isRenotify)
				Mailer.ClientRegistrationResened(client);
		}

		private List<string> GetEmailsForNotification(Client client)
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var dataAdapter = new MySqlDataAdapter(@"
select c.contactText
from usersettings.clientsdata cd
  join contacts.contact_groups cg on cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
    join contacts.contacts c on cg.Id = c.ContactOwnerId
where length(c.contactText) > 0
      and firmcode in (select pd.FirmCode
                        from pricesdata as pd, pricesregionaldata as prd
                        where pd.enabled = 1
                              and prd.enabled = 1
                              and firmstatus = 1
                              and firmtype = 0
                              and firmsegment = 0
                              and MaskRegion & ?Region >0)
      and cg.Type = ?ContactGroupType
      and c.Type = ?ContactType

union

select c.contactText
from usersettings.clientsdata cd
  join contacts.contact_groups cg on cd.ContactGroupOwnerId = cg.ContactGroupOwnerId
    join contacts.persons p on cg.id = p.ContactGroupId
      join contacts.contacts c on p.Id = c.ContactOwnerId
where length(c.contactText) > 0
      and firmcode in (select pd.FirmCode
                        from pricesdata as pd, pricesregionaldata as prd
                        where pd.enabled = 1
                              and prd.enabled = 1
                              and firmstatus = 1
                              and firmtype = 0
                              and firmsegment = 0
                              and MaskRegion & ?Region > 0)
      and cg.Type = ?ContactGroupType
      and c.Type = ?ContactType;", connection);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?Region", client.HomeRegion.Id);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?ContactGroupType", ContactGroupType.ClientManagers);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?ContactType", ContactType.Email);
				var data  = new DataSet();
				dataAdapter.Fill(data);
				return data.Tables[0].Rows.Cast<DataRow>().Select(r => r["ContactText"].ToString()).ToList();
			}
		}

		public void SendNotificationToBillingAboutClientRegistration(Client client,
			string userName,
			PaymentOptions options,
			string appUrl)
		{
			var message = new MailMessage();
			message.To.Add("billing@analit.net");
			message.From = new MailAddress("register@analit.net");
			message.IsBodyHtml = true;
			message.Subject = "Регистрация нового клиента";


			var paymentOptions = "";
			if (options != null)
				paymentOptions = "<br>" + options.GetCommentForPayer().Replace("\r\n", "<br>");
			
			message.Body = String.Format(
@"Зарегистрирован новый клиент
<br>
Название: <a href='{5}Billing/edit.rails?clientCode={1}'>{0}</a>
<br>
Код: {1}
<br>
Биллинг код: {2}
<br>
Кем зарегистрирован: {3}
<br>
{4}", client.Name, client.Id, client.BillingInstance.PayerID, userName, paymentOptions, appUrl).Replace(Environment.NewLine, "");

			_sendMessage(message);
		}
	}
}
