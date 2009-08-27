using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Common.MySql;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class CopySynonym : Page
	{
		private readonly DataSet _data = new DataSet();

		protected void FindBT_Click(object sender, EventArgs e)
		{
			FindClient(FromTB.Text, "From");
			FindClient(ToTB.Text, "ToT");
			FromL.Text = _data.Tables["from"].Rows.Count.ToString();
			ToL.Text = _data.Tables["tot"].Rows.Count.ToString();
			if (_data.Tables["from"].Rows.Count > 0)
			{
				if (_data.Tables["from"].Rows.Count > 50)
				{
					LabelErr.Text = "Найдено более 50 записей \"От\". Уточните условие поиска.";
					return;
				}
				FromTB.Visible = false;
				FromDD.DataSource = _data.Tables["From"];
				FromDD.Visible = true;
				FromDD.DataBind();
			}
			if (_data.Tables["tot"].Rows.Count > 0)
			{
				if (_data.Tables["tot"].Rows.Count > 50)
				{
					LabelErr.Text = "Найдено более 50 записей \"Для\". Уточните условие поиска.";
					return;
				}
				ToTB.Visible = false;
				ToDD.DataSource = _data.Tables["ToT"];
				ToDD.Visible = true;
				ToDD.DataBind();
			}
			if (_data.Tables["tot"].Rows.Count > 0 & _data.Tables["from"].Rows.Count > 0)
			{
				SetBT.Enabled = true;
				FindBT.Enabled = false;
				SetBT.Visible = true;
			}
			else
			{
				SetBT.Visible = false;
			}
		}

		public void FindClient(string nameStr, string where)
		{
			With.Connection(
				c => {
					var dataAdapter = new MySqlDataAdapter(@"
SELECT  FirmCode as ClientCode, 
        convert(concat(FirmCode, '. ', ShortName) using cp1251) name 
FROM    clientsdata
WHERE   MaskRegion & ?MaskRegion > 0
        and firmtype = 1 
        and firmstatus = 1 
        and (shortname like ?NameStr
			or fullname like ?NameStr)", c);
					dataAdapter.SelectCommand.Parameters.AddWithValue("?NameStr", String.Format("%{0}%", nameStr));
					dataAdapter.SelectCommand.Parameters.AddWithValue("?MaskRegion", SecurityContext.Administrator.RegionMask);
					dataAdapter.Fill(_data, where);
				});
		}

		protected void SetBT_Click(object sender, EventArgs e)
		{
			var clientCode = Convert.ToInt32(ToDD.SelectedItem.Value); 
			var parentClientCode = Convert.ToInt32(FromDD.SelectedItem.Value);
			With.Transaction(
				(c, t) => {
					var command = new MySqlCommand(@"
INSERT INTO Usersettings.AnalitFReplicationInfo(UserId, FirmCode)
SELECT ouar.RowId, supplier.FirmCode
FROM usersettings.clientsdata as drugstore
  JOIN usersettings.OsUserAccessRight ouar on ouar.ClientCode = drugstore.FirmCode
       JOIN clientsdata supplier ON supplier.firmsegment = drugstore.firmsegment
       LEFT JOIN Usersettings.AnalitFReplicationInfo ari on ari.UserId = ouar.RowId and ari.FirmCode = supplier.FirmCode
WHERE ari.UserId IS NULL
      AND supplier.firmtype = 0
      AND drugstore.FirmCode = ?ClientCode 
      AND drugstore.firmtype = 1
      AND supplier.maskregion & drugstore.maskregion > 0
group by ouar.RowId, supplier.FirmCode;

set @inHost = ?Host;
set @inUser = ?UserName;

UPDATE AnalitFReplicationInfo ari
	JOIN OsUserAccessRight ouar on ari.UserId = ouar.RowId
SET MaxSynonymCode = 0,
	MaxSynonymFirmCrCode = 0,
	ForceReplication = 1
WHERE ouar.ClientCode = ?ClientCode;

UPDATE (UserUpdateInfo as source, UserUpdateInfo as dest)
	JOIN OsUserAccessRight sourceUsers on sourceUsers.RowId = source.UserId
    JOIN OsUserAccessRight destUsers on destUsers.RowId = dest.UserId
SET dest.UpdateDate = source.UpdateDate,
	dest.AFAppVersion = source.AFAppVersion
WHERE sourceUsers.clientcode = ?ParentClientCode
      AND destUsers.clientcode = ?ClientCode;

UPDATE AnalitFReplicationInfo as dest
	JOIN AnalitFReplicationInfo as source on source.FirmCode = dest.FirmCode
		JOIN OsUserAccessRight sourceUsers on source.UserId = sourceUsers.RowId
	JOIN OsUserAccessRight destUsers on dest.UserId = destUsers.RowId
SET dest.MaxSynonymFirmCrCode = source.MaxSynonymFirmCrCode,
    dest.MaxSynonymCode = source.MaxSynonymCode
WHERE destUsers.clientcode = ?ClientCode
      AND sourceUsers.clientcode = ?ParentClientCode;

INSERT 
INTO    logs.clone 
        (
                LogTime, 
                UserName, 
                FromClientCode, 
                ToClientCode
        ) 
        VALUES 
        (
                now(), 
                ?UserName, 
                ?ParentClientCode, 
                ?ClientCode
        );", c, t);
					command.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
					command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
					command.Parameters.AddWithValue("?ClientCode", clientCode);
					command.Parameters.AddWithValue("?ParentClientCode", parentClientCode);

					command.ExecuteNonQuery();
				});

			NotificationHelper.NotifyAboutRegistration(
				String.Format("Успешное клонирование({0} > {1})", parentClientCode, clientCode),
				String.Format("От: {0} \nДля: {1} \nОператор: {2}", FromDD.SelectedItem.Text, ToDD.SelectedItem.Text, SecurityContext.Administrator.UserName));

			LabelErr.ForeColor = Color.Green;
			LabelErr.Text = "Клонирование успешно завершено.Время операции: " + DateTime.Now;
			FromDD.Visible = false;
			ToDD.Visible = false;
			FromTB.Visible = true;
			ToTB.Visible = true;
			FindBT.Visible = true;
			FindBT.Enabled = true;
			SetBT.Visible = false;
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			SecurityContext.Administrator.CheckPermisions(PermissionType.CopySynonyms, PermissionType.ViewDrugstore);
			
			With.Connection(
				c => {
					var adapter = new MySqlDataAdapter(@"
SELECT  r.region, 
        r.regioncode
FROM farm.regions r
WHERE r.RegionCode & ?MaskRegion > 0
ORDER BY region;", c);
					adapter.SelectCommand.Parameters.AddWithValue("?MaskRegion", SecurityContext.Administrator.RegionMask);
					adapter.Fill(_data, "Regions");
				});

			if (!IsPostBack)
			{
				RegionDD.DataSource = _data;
				RegionDD.DataBind();
			}
		}
	}
}
