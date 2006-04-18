using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using ActiveDs;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class searchc : Page
	{
		MySqlConnection соединение = new MySqlConnection(Literals.GetConnectionString());
		MySqlCommand Комманда = new MySqlCommand();
		MySqlDataReader Reader;
		TableRow row = new TableRow();
		TableCell cell = new TableCell();
		HyperLink HL = new HyperLink();

		private void Finder()
		{
			IADsUser ADUser;
			соединение.Open();
			Комманда.Connection = соединение;
			if (FindRB.SelectedItem.Value == "0")
			{
				Комманда.Parameters.Add(new MySqlParameter("Name", MySqlDbType.VarChar));
				Комманда.Parameters["Name"].Value = "%" + FindTB.Text + "%";
			}
			if (FindRB.SelectedItem.Value == "1")
			{
				Комманда.Parameters.Add(new MySqlParameter("ClientCode", MySqlDbType.Int32));
				Комманда.Parameters["ClientCode"].Value = FindTB.Text;
			}
			if (FindRB.SelectedItem.Value == "2")
			{
				Комманда.Parameters.Add(new MySqlParameter("Login", MySqlDbType.VarChar));
				Комманда.Parameters["Login"].Value = "%" + FindTB.Text + "%";
			}
			if (FindRB.SelectedItem.Value == "3")
			{
				Комманда.Parameters.Add(new MySqlParameter("BillingCode", MySqlDbType.Int32));
				Комманда.Parameters["BillingCode"].Value = FindTB.Text;
			}
			Комманда.CommandText = " SELECT cd. billingcode, cd.firmcode, ShortName, region, concat(' ', max(datecurprice)) FirstUpdate, concat(' ', max(dateprevprice)) SecondUpdate, null EXE, null MDB, " + " if(ouar2.rowid is null, ouar.OSUSERNAME, ouar2.OSUSERNAME) as UserName," + " FirmSegment, FirmType, (Firmstatus=0 or Billingstatus=0) Firmstatus," + " if(ouar2.rowid is null, ouar.rowid, ouar2.rowid) as ouarid, cd.firmcode as bfc" + " FROM (clientsdata as cd, farm.regions, accessright.showright, pricesdata, farm.formrules)" + " left join showregulation on ShowClientCode=cd.firmcode" + " left join osuseraccessright as ouar2 on ouar2.clientcode=cd.firmcode" + " left join osuseraccessright as ouar on ouar.clientcode=if(primaryclientcode is null, cd.firmcode, primaryclientcode)" + " where formrules.firmcode=pricesdata.pricecode" + " and pricesdata.firmcode=cd.firmcode" + " and regions.regioncode=cd.regioncode" + " and cd.regioncode & showright.regionmask > 0" + " and showright.UserName='" + Session["UserName"] + "'" + " and if(ShowOpt=1, FirmType=0, 0)" + " and if(UseRegistrant=1, Registrant=showright.UserName, 1)";
			if (FindRB.SelectedItem.Value == "0")
			{
				Комманда.CommandText += " and (cd.shortname like ?Name or cd.fullname like ?Name)";
			}
			if (FindRB.SelectedItem.Value == "1")
			{
				Комманда.CommandText += " and cd.firmcode=?ClientCode";
			}
			if (FindRB.SelectedItem.Value == "2")
			{
				Комманда.CommandText += " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)";
			}
			if (FindRB.SelectedItem.Value == "3")
			{
				Комманда.CommandText += " and cd.billingcode=?BillingCode";
			}
			Комманда.CommandText += " group by cd.firmcode" + " union" + " SELECT cd. billingcode, if(includeregulation.PrimaryClientCode is null, cd.firmcode, concat(cd.firmcode, '[', includeregulation.PrimaryClientCode, ']')), " + " if(includeregulation.PrimaryClientCode is null, cd.ShortName, concat(cd.ShortName, '[', incd.shortname, ']'))," + " region, UpdateTime FirstUpdate, UncommittedUpdateTime SecondUpdate, " + " EXEVersion as EXE, MDBVersion MDB, " + " if(ouar2.rowid is null, ouar.OSUSERNAME, ouar2.OSUSERNAME) as UserName," + " cd.FirmSegment, cd.FirmType, (cd.Firmstatus=0 or cd.Billingstatus=0) Firmstatus," + " if(ouar2.rowid is null, ouar.rowid, ouar2.rowid) as ouarid, cd.firmcode as bfc" + " FROM (clientsdata as cd, farm.regions, accessright.showright, retclientsset as rts)" + " left join showregulation on ShowClientCode=cd.firmcode" + " left join includeregulation on includeclientcode=cd.firmcode" + " left join clientsdata incd on incd.firmcode=includeregulation.PrimaryClientCode" + " left join osuseraccessright as ouar2 on ouar2.clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode)" + " left join osuseraccessright as ouar on ouar.clientcode=ifnull(showregulation.primaryclientcode, cd.firmcode)" + " left join logs.prgdataex on prgdataex.clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode)" + " and prgdataex.rowid=(select max(rowid) from logs.prgdataex where clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode) and updatetype in(1,2))" + " where" + " rts.clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode)" + " and regions.regioncode=cd.regioncode" + " and cd.regioncode & showright.regionmask > 0" + " and showright.UserName='" + Session["UserName"] + "'" + " and if(ShowRet=1, cd.FirmType=1, 0)" + " and if(UseRegistrant=1, cd.Registrant=showright.UserName, 1)";
			if (FindRB.SelectedItem.Value == "0")
			{
				Комманда.CommandText += " and (cd.shortname like ?Name or cd.fullname like ?Name)";
			}
			if (FindRB.SelectedItem.Value == "1")
			{
				Комманда.CommandText += " and cd.firmcode=?ClientCode";
			}
			if (FindRB.SelectedItem.Value == "2")
			{
				Комманда.CommandText += " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)";
			}
			if (FindRB.SelectedItem.Value == "3")
			{
				Комманда.CommandText += " and cd.billingcode=?BillingCode";
			}
			Комманда.CommandText += " group by cd.firmcode" + " order by 3, 4";
			DateTime StTime = DateTime.Now;
			Reader = Комманда.ExecuteReader();
			while (Reader.Read())
			{
				row = new TableRow();
				if (Reader["FirmStatus"].ToString() == "1")
				{
					row.BackColor = Color.FromArgb(255, 102, 0);
				}
				for (int i = 0; i <= Table3.Rows[0].Cells.Count - 1; i++)
				{
					try
					{
						cell = new TableCell();
						if ((i == 8) && (Reader[i].ToString().Length > 0))
						{
							if (ADCB.Checked)
							{
								try
								{
									ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + Reader[i].ToString()) as IADsUser;
									if (ADUser.IsAccountLocked)
									{
										cell.BackColor = Color.Violet;
									}
									if (ADUser.AccountDisabled)
									{
										cell.BackColor = Color.Aqua;
									}
								}
								catch
								{
									cell.BackColor = Color.Red;
								}
							}
							cell.Text = Reader[i].ToString();
						}
						else if (i == 2)
						{
							HL = new HyperLink();
							HL.Text = Reader[i].ToString();
							HL.NavigateUrl = "info.aspx?cc=" + Reader["bfc"] + "&ouar=" + Reader["ouarid"];
							cell.Controls.Add(HL);
						}
						else if (i == 9)
						{
							if (Reader[i].ToString() == "0")
							{
								cell.Text = "Опт";
							}
							else
							{
								cell.Text = "Справка";
							}
						}
						else if (i == 10)
						{
							if (Reader[i].ToString() == "1")
							{
								cell.Text = "Аптека";
							}
							else
							{
								cell.Text = "Поставщик";
							}
						}
						else if (i == 4 | i == 5)
						{
							DateTime PriceDate = Convert.ToDateTime(Reader[i]);
							if (i == 4)
							{
								if ((DateTime.Now.Subtract(PriceDate).TotalDays > 2) && (Reader["FirmStatus"].ToString() == "0"))
								{
									cell.BackColor = Color.Gray;
								}
							}
							cell.Text = PriceDate.ToString("dd.MM.yy HH:mm");
							cell.HorizontalAlign = HorizontalAlign.Center;
						}
						else
						{
							cell.Text = Reader[i].ToString();
						}
					}
					catch (Exception)
					{
						cell = new TableCell();
						cell.Text = Reader[i].ToString();
					}
					row.Cells.Add(cell);
				}
				Table3.Rows.Add(row);
				TimeSLB.Text = DateTime.Now.Subtract(StTime).ToString();
			}
			if (Table3.Rows.Count > 1)
			{
				Table3.Visible = true;
				Table4.Visible = true;
				Label1.Visible = false;
			}
			else
			{
				Label1.Visible = true;
				Table3.Visible = false;
				Table4.Visible = false;
			}
			Reader.Close();
			соединение.Close();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
/*			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");
			
			соединение.Open();
			Комманда.CommandText = "select max(UserName='" + Session["UserName"] + "') from accessright.showright";
			Комманда.Connection = соединение;
			if (Комманда.ExecuteScalar().ToString() == "0")
			{
				Session["strError"] = "Пользователь " + Session["UserName"] + " не найден!";
				соединение.Close();
				Response.Redirect("error.aspx");
			}
			соединение.Close();*/
		}
		
		protected void GoFind_Click(object sender, EventArgs e)
		{
			Finder();
		}
}
}