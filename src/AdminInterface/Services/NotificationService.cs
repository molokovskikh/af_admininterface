using System;
using System.Data;
using System.Net.Mail;
using AdminInterface.Helpers;
using AdminInterface.Models;
using Common.Web.Ui.Models;
using MySql.Data.MySqlClient;

namespace AdminInterface.Services
{
	public class NotificationService
	{
		private readonly Action<MailMessage> _sendMessage;

		public NotificationService(Action<MailMessage> sendMessage)
		{
			_sendMessage = sendMessage;
		}

		public NotificationService()
		{
			_sendMessage = Func.SendWitnStandartSender;
		}

		public void NotifySupplierAboutDrugstoreRegistration(Client client)
		{
			var data = new DataSet();
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
				dataAdapter.Fill(data);
			}
			foreach (DataRow Row in data.Tables[0].Rows)
				NotificationHelper.NotifySupplierAboutDrugstoreRegistration(client,
					Row["ContactText"].ToString());
		}

		public void SendNotificationToBillingAboutClientRegistration(Client client,
			string userName,
			PaymentOptions options)
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
Название: <a href='https://stat.analit.net/Adm/Billing/edit.rails?clientCode={1}'>{0}</a>
<br>
Код: {1}
<br>
Биллинг код: {2}
<br>
Кем зарегистрирован: {3}
<br>
{4}", client.ShortName, client.Id, client.BillingInstance.PayerID, userName, paymentOptions).Replace(Environment.NewLine, "");

			_sendMessage(message);
		}
	}
}
