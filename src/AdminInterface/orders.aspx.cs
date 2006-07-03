using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class orders : Page
	{
		MySqlConnection ���������� = new MySqlConnection();
		MySqlCommand �������� = new MySqlCommand();
		MySqlDataReader Reader;
		Int64 ClientCode;
		TableRow row = new TableRow();
		TableCell cell = new TableCell();

		protected void Button1_Click(object sender, EventArgs e)
		{
			����������.Open();
			ClientCode = Convert.ToInt32(Request["cc"]);
			��������.Connection = ����������;
			��������.CommandText =
				" SELECT ordershead.rowid, WriteTime, PriceDate, client.shortname, firm.shortname, PriceName, RowCount, Processed" +
				" FROM orders.ordershead, usersettings.pricesdata, usersettings.clientsdata as firm, usersettings.clientsdata as client, usersettings.clientsdata as sel where " +
				" clientcode=client.firmcode" + " and client.firmcode=if(sel.firmtype=1, sel.firmcode, client.firmcode)" +
				" and firm.firmcode=if(sel.firmtype=0, sel.firmcode, firm.firmcode)" +
				" and writetime between ?FromDate and ?ToDate and pricesdata.pricecode=ordershead.pricecode" +
				" and firm.firmcode=pricesdata.firmcode and sel.firmcode=?clientCode order by writetime desc";
			��������.Parameters.Add(new MySqlParameter("FromDate", MySqlDbType.Datetime));
			��������.Parameters["FromDate"].Value = CalendarFrom.SelectedDate;
			��������.Parameters.Add(new MySqlParameter("ToDate", MySqlDbType.Datetime));
			��������.Parameters["ToDate"].Value = CalendarTo.SelectedDate;
			��������.Parameters.Add("?clientCode", ClientCode);
			Reader = ��������.ExecuteReader();
			while (Reader.Read())
			{
				row = new TableRow();
				if (Convert.ToInt32(Reader[6]) == 0)
				{
					row.BackColor = Color.Red;
				}
				for (int i = 0; i <= Table3.Rows[0].Cells.Count - 1; i++)
				{
					cell = new TableCell();
					if (i == 1 | i == 2)
					{
						cell.Text = Convert.ToDateTime(Reader[i]).ToString();
						cell.HorizontalAlign = HorizontalAlign.Center;
					}
					else
					{
						cell.Text = Reader[i].ToString();
					}
					row.Cells.Add(cell);
				}
				Table3.Rows.Add(row);
			}
			Reader.Close();
			����������.Close();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			����������.ConnectionString = Literals.GetConnectionString();
			if (!(Page.IsPostBack))
			{
				CalendarFrom.SelectedDate = DateTime.Now.AddDays(-7);
				CalendarTo.SelectedDate = DateTime.Now.AddDays(1);
			}
		}
	}
}