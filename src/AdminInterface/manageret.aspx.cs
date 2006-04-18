using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class manageret : Page
	{
		protected DataSet DS1;
		protected DataTable Regions;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		protected DataTable WorkReg;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn7;
		protected DataColumn DataColumn8;
		protected DataTable Clientsdata;
		protected DataColumn DataColumn9;
		protected DataColumn DataColumn10;
		protected DataColumn DataColumn11;
		protected DataColumn DataColumn12;
		protected DataColumn DataColumn13;
		protected DataColumn DataColumn14;
		protected DataColumn DataColumn15;
		protected DataColumn DataColumn16;
		protected DataColumn DataColumn17;
		protected DataColumn DataColumn18;
		protected DataColumn DataColumn19;
		protected DataColumn DataColumn20;
		protected DataColumn DataColumn21;
		protected DataTable admin;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected DataTable RetClientsSet;
		MySqlConnection myMySqlConnection = new MySqlConnection(Literals.GetConnectionString());
		MySqlCommand myMySqlCommand = new MySqlCommand();
		MySqlDataReader myMySqlDataReader;
		MySqlTransaction myTrans;
		object ClientCode;
		object HomeRegionCode;
		object WorkMask;
		object ShowMask;
		Int64 OrderMask;
		string InsertCommand;


		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			DS1 = new DataSet();
			Regions = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			WorkReg = new DataTable();
			DataColumn5 = new DataColumn();
			DataColumn7 = new DataColumn();
			DataColumn8 = new DataColumn();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			Clientsdata = new DataTable();
			DataColumn9 = new DataColumn();
			DataColumn10 = new DataColumn();
			DataColumn11 = new DataColumn();
			DataColumn12 = new DataColumn();
			DataColumn13 = new DataColumn();
			DataColumn14 = new DataColumn();
			DataColumn15 = new DataColumn();
			DataColumn16 = new DataColumn();
			DataColumn17 = new DataColumn();
			DataColumn18 = new DataColumn();
			DataColumn19 = new DataColumn();
			DataColumn20 = new DataColumn();
			DataColumn21 = new DataColumn();
			admin = new DataTable();
			RetClientsSet = new DataTable();
			DS1.BeginInit();
			Regions.BeginInit();
			WorkReg.BeginInit();
			Clientsdata.BeginInit();
			admin.BeginInit();
			RetClientsSet.BeginInit();
			DS1.DataSetName = "NewDataSet";
			DS1.Locale = new CultureInfo("ru-RU");
			DS1.Tables.AddRange(new DataTable[] {Regions, WorkReg, Clientsdata, admin, RetClientsSet});
			Regions.Columns.AddRange(new DataColumn[] {DataColumn1, DataColumn2});
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			DataColumn2.DataType = typeof (Int64);
			WorkReg.Columns.AddRange(new DataColumn[] {DataColumn5, DataColumn7, DataColumn8, DataColumn3, DataColumn4});
			WorkReg.TableName = "WorkReg";
			DataColumn5.ColumnName = "RegionCode";
			DataColumn5.DataType = typeof (Int32);
			DataColumn7.ColumnName = "ShowMask";
			DataColumn7.DataType = typeof (Boolean);
			DataColumn8.ColumnName = "RegMask";
			DataColumn8.DataType = typeof (Boolean);
			DataColumn3.ColumnName = "Region";
			DataColumn4.ColumnName = "OrderMask";
			DataColumn4.DataType = typeof (Boolean);
			Clientsdata.Columns.AddRange(
				new DataColumn[]
					{
						DataColumn9, DataColumn10, DataColumn11, DataColumn12, DataColumn13, DataColumn14, DataColumn15, DataColumn16,
						DataColumn17, DataColumn18, DataColumn19, DataColumn20, DataColumn21
					});
			Clientsdata.TableName = "Clientsdata";
			DataColumn9.ColumnName = "adress";
			DataColumn10.ColumnName = "bussinfo";
			DataColumn11.ColumnName = "bussstop";
			DataColumn12.ColumnName = "fax";
			DataColumn13.ColumnName = "firmsegment";
			DataColumn13.DataType = typeof (Int16);
			DataColumn14.ColumnName = "firmtype";
			DataColumn14.DataType = typeof (Int16);
			DataColumn15.ColumnName = "oldcode";
			DataColumn15.DataType = typeof (Int16);
			DataColumn16.ColumnName = "phone";
			DataColumn17.ColumnName = "regioncode";
			DataColumn17.DataType = typeof (Int64);
			DataColumn18.ColumnName = "shortname";
			DataColumn19.ColumnName = "url";
			DataColumn20.ColumnName = "fullname";
			DataColumn21.ColumnName = "mail";
			admin.TableName = "admin";
			RetClientsSet.TableName = "RetClientsSet";
			DS1.EndInit();
			Regions.EndInit();
			WorkReg.EndInit();
			Clientsdata.EndInit();
			admin.EndInit();
			RetClientsSet.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		protected void ParametersSave_Click(object sender, EventArgs e)
		{
			if ((AlowCumulativeCB.Checked) && (CUWTB.Text.Length < 5) && (AlowCumulativeCB.Enabled))
			{
				ResultL.Text = "Изменения не сохранены.<br>Укажите причину кумулятивного обновления.";
				ResultL.ForeColor = Color.Red;
				return;
			}
			if (ResetCopyIDCB.Checked & CopyIDWTB.Text.Length < 5 & ResetCopyIDCB.Enabled)
			{
				ResultL.Text = "Изменения не сохранены.<br>Укажите причину сброса идентификатора.";
				ResultL.ForeColor = Color.Red;
				return;
			}
			myMySqlConnection.Open();
			myTrans = myMySqlConnection.BeginTransaction();
			myMySqlCommand.Transaction = myTrans;
			myMySqlCommand.Parameters.Add(new MySqlParameter("InvisibleOnFirm", MySqlDbType.Int16));
			myMySqlCommand.Parameters["InvisibleOnFirm"].Value = InvisibleCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AlowRegister", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AlowRegister"].Value = RegisterCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AlowRejection", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AlowRejection"].Value = RejectsCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AlowDocuments", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AlowDocuments"].Value = DocumentsCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("MultiUserLevel", MySqlDbType.Int16));
			myMySqlCommand.Parameters["MultiUserLevel"].Value = MultiUserLevelTB.Text;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AdvertisingLevel", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AdvertisingLevel"].Value = AdvertisingLevelCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AlowWayBill", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AlowWayBill"].Value = WayBillCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AlowChangeSegment", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AlowChangeSegment"].Value = ChangeSegmentCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("EnableUpdate", MySqlDbType.Int16));
			myMySqlCommand.Parameters["EnableUpdate"].Value = EnableUpdateCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("CheckCopyID", MySqlDbType.Int16));
			myMySqlCommand.Parameters["CheckCopyID"].Value = CheckCopyIDCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("AlowCumulativeUpdate", MySqlDbType.Int16));
			myMySqlCommand.Parameters["AlowCumulativeUpdate"].Value = AlowCumulativeCB.Checked;
			myMySqlCommand.Parameters.Add(new MySqlParameter("ResetIDCause", MySqlDbType.String));
			myMySqlCommand.Parameters["ResetIDCause"].Value = CopyIDWTB.Text;
			myMySqlCommand.Parameters.Add(new MySqlParameter("CumulativeUpdateCause", MySqlDbType.String));
			myMySqlCommand.Parameters["CumulativeUpdateCause"].Value = CUWTB.Text;
			HomeRegionCode = RegionDD.SelectedItem.Value;
			for (int i = 0; i <= ShowList.Items.Count - 1; i++)
			{
				if (ShowList.Items[i].Selected)
				{
					ShowMask = ShowMask + ShowList.Items[i].Value;
				}
				if (WRList.Items[i].Selected)
				{
					WorkMask = WorkMask + WRList.Items[i].Value;
				}
				if (OrderList.Items[i].Selected)
				{
					OrderMask = OrderMask + Convert.ToInt32(OrderList.Items[i].Value);
				}
			}
			try
			{
				myMySqlCommand.CommandText = "select RegionCode=?homeRegionCode and ShowRegionMask=?showMask and MaskRegion=?workMask from usersettings.clientsdata where firmcode=?clientCode";

				myMySqlCommand.Parameters.Add("?clientCode", ClientCode);
				myMySqlCommand.Parameters.Add("?workMask", WorkMask);
				myMySqlCommand.Parameters.Add("?homeRegionCode", HomeRegionCode);
				myMySqlCommand.Parameters.Add("?showMask", ShowMask);
				myMySqlCommand.Parameters.Add("?userName", Session["UserName"]);
				myMySqlCommand.Parameters.Add("?userHostAddress", HttpContext.Current.Request.UserHostAddress);
				myMySqlCommand.Parameters.Add("?orderMask", OrderMask);
				
				if (myMySqlCommand.ExecuteScalar() == "0")
					InsertCommand += "insert into logs.clientsdataupdates select null, now(), ?userName, ?userHostAddress, ?clientCode, null, null, null, if(RegionCode=?homeRegionCode, null, ?homeRegionCode), if(MaskRegion=?workMask, null, ?workMask), if(ShowRegionMask=?showMask, null, ?showMask), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null from clientsdata where firmcode=?clientCode; ";

				myMySqlCommand.CommandText = "select MaskRegion=?workMask from clientsdata where firmcode=?clientCode";
				if (myMySqlCommand.ExecuteScalar() == "0")
				{
					InsertCommand = InsertCommand +
					                " insert into intersection(ClientCode, regioncode, pricecode, invisibleonclient, InvisibleonFirm, CostCode)" +
					                " SELECT distinct clientsdata2.firmcode, regions.regioncode, pricesdata.pricecode," +
					                " pricesdata.PriceType=2 as invisibleonclient, a.invisibleonfirm, (SELECT costcode FROM pricescosts pcc WHERE basecost" +
					                " AND showpricecode=pc.showpricecode)" +
					                " FROM (clientsdata, farm.regions, pricesdata, pricesregionaldata, pricescosts pc)" +
									" left join clientsdata as clientsdata2 on clientsdata2.firmcode=?clientCode" +
					                " LEFT JOIN intersection ON intersection.pricecode=pricesdata.pricecode and intersection.regioncode=regions.regioncode and intersection.clientcode=clientsdata2.firmcode" +
					                " left join retclientsset as a on a.clientcode=clientsdata2.firmcode" +
					                " WHERE intersection.pricecode IS NULL and " + " clientsdata.firmstatus=1 " +
					                " and clientsdata.firmsegment=clientsdata2.firmsegment" + " and clientsdata.firmtype=0" +
					                " and pricesdata.firmcode=clientsdata.firmcode" +
					                " and pricesregionaldata.pricecode=pricesdata.pricecode" +
					                " and pricesregionaldata.regioncode=regions.regioncode" + " and pricesdata.pricetype<>1" +
					                " AND pricesdata.pricecode=pc.showpricecode " +
									" and (clientsdata.maskregion & regions.regioncode)>0" + " and (?workMask" +
					                " & regions.regioncode)>0;";
				}
				myMySqlCommand.CommandText = "select OrderRegionMask=?orderMask" +
				                             " and InvisibleOnFirm=?InvisibleOnFirm and AlowRegister=?AlowRegister and AlowRejection=?AlowRejection and AlowDocuments=?AlowDocuments and MultiUserLevel=?MultiUserLevel and (WorkRegionMask=if(WorkRegionMask & " +
											 "?workMask > 0, WorkRegionMask, ?homeRegionCode))" +
				                             "and AdvertisingLevel=?AdvertisingLevel and AlowWayBill=?AlowWayBill and AlowChangeSegment=?AlowChangeSegment and EnableUpdate=?EnableUpdate and CheckCopyID=?CheckCopyID " +
											 " from retclientsset where clientcode=?clientCode";
				if ((myMySqlCommand.ExecuteScalar() == "0")
				    || (((AlowCumulativeCB.Enabled) && (AlowCumulativeCB.Checked))
				        || ((ResetCopyIDCB.Enabled) && (ResetCopyIDCB.Checked))))
				{
					InsertCommand = InsertCommand + 
									"insert into logs.retclientssetupdate select null, now(), ?userName , ?userHostAddress, ?clientCode " +
									", if(InvisibleOnFirm=?InvisibleOnFirm, null, ?InvisibleOnFirm), null, null, null, null, if(AlowRegister=?AlowRegister, null, ?AlowRegister), if(AlowRejection=?AlowRejection, null, ?AlowRejection), " +
					                "if(AlowDocuments=?AlowDocuments, null, ?AlowDocuments), if(MultiUserLevel=?MultiUserLevel, null, ?MultiUserLevel), if(AdvertisingLevel=?AdvertisingLevel, null, ?AdvertisingLevel), " +
					                "if(AlowWayBill=?AlowWayBill, null, ?AlowWayBill), if(AlowChangeSegment=?AlowChangeSegment, null, ?AlowChangeSegment), null, if(WorkRegionMask=if(WorkRegionMask & " +
									"?workMask > 0, WorkRegionMask, ?homeRegionCode), null, ?homeRegionCode), if(OrderRegionMask=?orderMask, null, ?orderMask" +
					                "), null, null, if(EnableUpdate=?EnableUpdate, null, ?EnableUpdate), if(CheckCopyID=?CheckCopyID, null, ?CheckCopyID), ";
					if (AlowCumulativeCB.Enabled & AlowCumulativeCB.Checked)
					{
						InsertCommand += "?CumulativeUpdateCause, ";
					}
					else
					{
						InsertCommand += "null, ";
					}
					if (ResetCopyIDCB.Enabled & ResetCopyIDCB.Checked)
					{
						InsertCommand += "?ResetIDCause ";
					}
					else
					{
						InsertCommand += "null ";
					}
					InsertCommand += ",null, null, null from retclientsset where clientcode=?clientCode; ";
				}
				if (InvisibleCB.Enabled)
				{
					myMySqlCommand.CommandText = "select InvisibleOnFirm=?InvisibleOnFirm from retclientsset where clientcode=?clientCode";
					if (Convert.ToInt32(myMySqlCommand.ExecuteScalar()) == 0)
					{
						InsertCommand += " update retclientsset, intersection, pricesdata set retclientsset.invisibleonfirm=?InvisibleOnFirm, intersection.invisibleonfirm=?InvisibleOnFirm";
						if (InvisibleCB.Checked)
						{
							InsertCommand += ", DisabledByFirm=if(PriceType=2, 1, 0), InvisibleOnClient=if(PriceType=2, 1, 0)";
						}
						InsertCommand += " where intersection.clientcode=retclientsset.clientcode and intersection.pricecode=pricesdata.pricecode and intersection.clientcode=?clientCode; ";
					}
				}
				InsertCommand += "update retclientsset, clientsdata set OrderRegionMask=?orderMask, MaskRegion=?workMask" +
								 ", ShowRegionMask=?showMask, RegionCode=?homeRegionCode" +
								 ", WorkRegionMask=if(WorkRegionMask & ?workMask > 0, WorkRegionMask, ?homeRegionCode), " +
				                 " AlowRegister=?AlowRegister, AlowRejection=?AlowRejection, AlowDocuments=?AlowDocuments, MultiUserLevel=?MultiUserLevel, " +
				                 "AdvertisingLevel=?AdvertisingLevel, AlowWayBill=?AlowWayBill, AlowChangeSegment=?AlowChangeSegment, EnableUpdate=?EnableUpdate, CheckCopyID=?CheckCopyID, AlowCumulativeUpdate=?AlowCumulativeUpdate";
				if (ResetCopyIDCB.Enabled & ResetCopyIDCB.Checked)
				{
					InsertCommand += ", UniqueCopyID=''";
				}
				InsertCommand += " where clientcode=firmcode and firmcode=?clientCode";
				myMySqlCommand.CommandText = InsertCommand;
				myMySqlCommand.ExecuteNonQuery();
				myTrans.Commit();
			}
			catch (Exception err)
			{
				myTrans.Rollback();
				ResultL.ForeColor = Color.Red;
				ResultL.Text = err.Message;
				return;
			}
			finally
			{
				myMySqlConnection.Close();
			}
			ResultL.ForeColor = Color.Green;
			ResultL.Text = "Сохранено.";
		}

		protected void SendMessage_Click(object sender, EventArgs e)
		{
			myMySqlConnection.Open();
			myTrans = myMySqlConnection.BeginTransaction();
			myMySqlCommand.Transaction = myTrans;
			try
			{
				myMySqlCommand.CommandText = "insert into logs.retclientssetupdate (RowID, LogTime, OperatorName, OperatorHost, ClientCode, ShowMessageCount, Message) select null, now(), '"
				                             + Session["UserName"] + "', '" + HttpContext.Current.Request.UserHostAddress + "', "
				                             + ClientCode + ", " + SendMessageCountDD.SelectedItem.Value + ", ?Message; "
				                             + "update retclientsset set ShowMessageCount="
				                             + SendMessageCountDD.SelectedItem.Value + ", Message=?Message where clientcode="
				                             + ClientCode;
				myMySqlCommand.Parameters.Add("Message", MySqlDbType.String);
				myMySqlCommand.Parameters[0].Value = MessageTB.Text;
				myMySqlCommand.ExecuteNonQuery();
				myTrans.Commit();
				MessageTB.Text = "";
				StatusL.Visible = true;
			}
			catch (Exception err)
			{
				myTrans.Rollback();
				StatusL.Text = err.Message;
				StatusL.ForeColor = Color.Red;
				StatusL.Visible = true;
				return;
			}
			finally
			{
				myMySqlConnection.Close();
			}
		}

		protected void RegionDD_SelectedIndexChanged(object sender, EventArgs e)
		{
			bool OldRegion = false;
			if (RegionDD.SelectedItem.Value == HomeRegionCode)
			{
				OldRegion = true;
			}
			SetWorkRegions(Convert.ToInt64(RegionDD.SelectedItem.Value), OldRegion, false);
		}

		private void SetWorkRegions(Int64 RegCode, bool OldRegion, bool AllRegions)
		{
			string SQLTXT;
			SQLTXT = " select a.RegionCode, a.Region, ShowRegionMask & a.regioncode>0 as ShowMask," +
			         " MaskRegion & a.regioncode>0 as RegMask, OrderRegionMask & a.regioncode>0 as OrderMask" +
			         " from farm.regions as a, farm.regions as b, clientsdata, retclientsset, accessright.regionaladmins" +
			         " where";
			if (!(AllRegions))
			{
				SQLTXT += " b.regioncode=" + RegCode + " and";
			}
			SQLTXT += " clientsdata.firmcode=" + ClientCode + " and a.regioncode & b.defaultshowregionmask>0 "
			          + " and clientcode=firmcode" + " and regionaladmins.username='"
			          + Session["UserName"] + "'" + " and a.regioncode & regionaladmins.RegionMask > 0"
			          + " group by regioncode" + " order by region";
			Func.SelectTODS(SQLTXT, "WorkReg", DS1);
			WRList.DataBind();
			OrderList.DataBind();
			ShowList.DataBind();
			for (int i = 0; i <= WRList.Items.Count - 1; i++)
			{
				if (OldRegion)
				{
					WRList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["RegMask"]);
					OrderList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["OrderMask"]);
					ShowList.Items[i].Selected = Convert.ToBoolean(DS1.Tables["Workreg"].Rows[i]["ShowMask"]);
				}
				else
				{
					ShowList.Items[i].Selected = true;
					if (WRList.Items[i].Value == RegCode.ToString())
					{
						WRList.Items[i].Selected = true;
					}
					OrderList.Items[i].Selected = WRList.Items[i].Selected;
				}
			}
		}

		protected void AllRegCB_CheckedChanged(object sender, EventArgs e)
		{
			if (AllRegCB.Checked)
			{
				SetWorkRegions(Convert.ToInt64(RegionDD.SelectedItem.Value), true, true);
			}
			else
			{
				SetWorkRegions(Convert.ToInt64(RegionDD.SelectedItem.Value), true, false);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			StatusL.Visible = false;
			ClientCode = Convert.ToInt32(Request["cc"]);
			myMySqlCommand.Connection = myMySqlConnection;
			myMySqlConnection.Open();
			myTrans = myMySqlConnection.BeginTransaction();
			myMySqlCommand.Transaction = myTrans;
			myMySqlCommand.CommandText = " SELECT RegionCode, MaskRegion, ShowRegionMask"
			                             + " FROM clientsdata as cd, accessright.regionaladmins"
			                             + " where cd.regioncode & regionaladmins.regionmask > 0 and UserName='"
			                             + Session["UserName"] + "'"
			                             +
			                             " and FirmType=if(AlowCreateRetail+AlowCreateVendor=2, FirmType, if(AlowCreateRetail=1, 1, 0))"
			                             + " and FirmSegment=if(regionaladmins.AlowChangeSegment=1, FirmSegment, DefaultSegment)"
			                             + "\n and if(UseRegistrant=1, Registrant='" + Session["UserName"] + "', 1=1)" +
			                             " and AlowManage=1 and cd.firmcode=" + ClientCode;
			HomeRegionCode = Convert.ToInt32(myMySqlCommand.ExecuteScalar());
			if (Convert.ToInt32(HomeRegionCode) < 1)
			{
				return;
			}
			if (!(IsPostBack))
			{
				Func.SelectTODS(
					"select regions.regioncode, regions.region from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" +
					Session["UserName"] + "' order by region", "admin", DS1);
				RegionDD.DataBind();
				for (int i = 0; i <= RegionDD.Items.Count - 1; i++)
				{
					if (RegionDD.Items[i].Value == HomeRegionCode)
					{
						RegionDD.SelectedIndex = i;
						goto exitForStatement0;
					}
				}
				exitForStatement0:
				;
				SetWorkRegions(Convert.ToInt64(HomeRegionCode), true, false);
				myMySqlCommand.CommandText =
					" select InvisibleOnFirm, AlowRegister, AlowRejection, AlowDocuments, MultiUserLevel," +
					" AdvertisingLevel, AlowWayBill, retclientsset.AlowChangeSegment, EnableUpdate, CheckCopyID," +
					" AlowCumulativeUpdate, AlowCreateInvisible, length(UniqueCopyID)=0 from retclientsset, accessright.regionaladmins where clientcode=" +
					ClientCode + " and username='" + Session["UserName"] + "'";
				myMySqlDataReader = myMySqlCommand.ExecuteReader();
				myMySqlDataReader.Read();
				InvisibleCB.Checked = Convert.ToBoolean(myMySqlDataReader[0]);
				InvisibleCB.Enabled = Convert.ToBoolean(myMySqlDataReader[11]);
				RegisterCB.Checked = Convert.ToBoolean(myMySqlDataReader[1]);
				RejectsCB.Checked = Convert.ToBoolean(myMySqlDataReader[2]);
				DocumentsCB.Checked = Convert.ToBoolean(myMySqlDataReader[3]);
				MultiUserLevelTB.Text = myMySqlDataReader[4].ToString();
				AdvertisingLevelCB.Checked = Convert.ToBoolean(myMySqlDataReader[5]);
				WayBillCB.Checked = Convert.ToBoolean(myMySqlDataReader[6]);
				ChangeSegmentCB.Checked = Convert.ToBoolean(myMySqlDataReader[7]);
				EnableUpdateCB.Checked = Convert.ToBoolean(myMySqlDataReader[8]);
				CheckCopyIDCB.Checked = Convert.ToBoolean(myMySqlDataReader[9]);
				AlowCumulativeCB.Checked = Convert.ToBoolean(myMySqlDataReader[10]);
				ResetCopyIDCB.Checked = Convert.ToBoolean(myMySqlDataReader[12]);
				if (!(AlowCumulativeCB.Checked))
				{
					AlowCumulativeCB.Enabled = true;
					CUWTB.Enabled = true;
					CUSetL.Visible = false;
				}
				if (!(ResetCopyIDCB.Checked))
				{
					ResetCopyIDCB.Enabled = true;
					CopyIDWTB.Enabled = true;
					IDSetL.Visible = false;
				}
				myMySqlDataReader.Close();
			}
			myTrans.Commit();
			myMySqlConnection.Close();
		}
	}
}