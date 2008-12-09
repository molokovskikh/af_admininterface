using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using MySql.Data.MySqlClient;

namespace AdminInterface
{
	public partial class SenderProperties : Page
	{
		private DataSet Data
		{
			get { return (DataSet)Session["RegionalSettingsData"]; }
			set { Session["RegionalSettingsData"] = value; }
		}

		private int RuleId
		{
			get { return Convert.ToInt32(Session["ClientCode"]); }
			set { Session["ClientCode"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StateHelper.CheckSession(this, ViewState);
			SecurityContext
				.Administrator
				.CheckPermisions(PermissionType.ViewSuppliers, PermissionType.ManageSuppliers);

			if (!IsPostBack)
			{
				RuleId = Int32.Parse(Request["RuleId"]);
				GetData();
				ConnectDataSource();
				DataBind();
			}
			else
				ConnectDataSource();
		}

		private void ConnectDataSource()
		{
			Properties.DataSource = Data.Tables["Properties"];
		}

		private void GetData()
		{
			var ruleInfo = @"
SELECT sender.ClassName as Sender, formater.ClassName as Formater, o.RegionCode
FROM ordersendrules.order_send_rules o
	JOIN ordersendrules.order_handlers sender on sender.Id = o.SenderId
	JOIN ordersendrules.order_handlers formater on formater.Id = o.FormaterId
WHERE o.Id = ?RuleId";

			var properties = @"
select hp.*
from ordersendrules.handler_properties hp
where OrderSendRuleId = ?RuleId
order by hp.name;";

			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var dataAdapter = new MySqlDataAdapter("", connection);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?RuleId", RuleId);
				Data = new DataSet();

				dataAdapter.SelectCommand.CommandText = properties;
				dataAdapter.Fill(Data, "Properties");

				dataAdapter.SelectCommand.CommandText = ruleInfo;
				dataAdapter.Fill(Data, "RuleInfo");

				if (Data.Tables["RuleInfo"].Rows[0]["RegionCode"] != DBNull.Value)
					SecurityContext.Administrator.CheckClientHomeRegion(Convert.ToUInt64(Data.Tables["RuleInfo"].Rows[0]["RegionCode"]));
			}

			Header.Text = String.Format("Настройка свойств для отправщика {0} и форматера {1}",
			                            Data.Tables["RuleInfo"].Rows[0]["Sender"],
			                            Data.Tables["RuleInfo"].Rows[0]["Formater"]);
		}

		protected void Save_Click(object sender, EventArgs e)
		{
			ProcessChanges();
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
				try
				{
					var dataAdapter = new MySqlDataAdapter("", connection);
					dataAdapter.InsertCommand = new MySqlCommand(@"
insert into ordersendrules.handler_properties(name, value, OrderSendRuleId)
values (?name, ?value, ?ruleId);", connection);
					dataAdapter.InsertCommand.Transaction = transaction;
					dataAdapter.InsertCommand.Parameters.Add("?Name", MySqlDbType.VarString, 0, "Name");
					dataAdapter.InsertCommand.Parameters.Add("?Value", MySqlDbType.VarString, 0, "Value");
					dataAdapter.InsertCommand.Parameters.AddWithValue("?RuleId", RuleId);

					dataAdapter.DeleteCommand = new MySqlCommand(@"
delete from ordersendrules.handler_properties
where id = ?id;", connection);
					dataAdapter.DeleteCommand.Transaction = transaction;
					dataAdapter.DeleteCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "Id");

					dataAdapter.UpdateCommand = new MySqlCommand(@"
update ordersendrules.handler_properties
set name = ?name,
	value = ?value
where id = ?id;", connection);
					dataAdapter.UpdateCommand.Transaction = transaction;
					dataAdapter.UpdateCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "Id");
					dataAdapter.UpdateCommand.Parameters.Add("?Name", MySqlDbType.VarString, 0, "Name");
					dataAdapter.UpdateCommand.Parameters.Add("?Value", MySqlDbType.VarString, 0, "Value");

					dataAdapter.Update(Data.Tables["Properties"]);

					transaction.Commit();
				}
				catch (Exception)
				{
					if (transaction != null)
						transaction.Rollback();

					throw;
				}
			}

			GetData();
			ConnectDataSource();
			DataBind();
		}

		private void ProcessChanges()
		{
			for (var i = 0; i < Properties.Rows.Count; i++)
			{
				var row = Properties.Rows[i];
				var dataRow = Data.Tables["Properties"].DefaultView[i];

				if (((TextBox)row.FindControl("Name")).Text != dataRow["Name"].ToString())
					dataRow["Name"] = ((TextBox) row.FindControl("Name")).Text;

				if (((TextBox)row.FindControl("Value")).Text != dataRow["Value"].ToString())
					dataRow["Value"] = ((TextBox)row.FindControl("Value")).Text;
			}
		}

		protected void Properties_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			Data.Tables["Properties"].DefaultView[e.RowIndex].Delete();
			((GridView)sender).DataBind();	
		}

		protected void Properties_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					var row = Data.Tables["Properties"].NewRow();
					Data.Tables["Properties"].Rows.Add(row);
					((GridView)sender).DataBind();
					break;
			}
		}
	}
}
