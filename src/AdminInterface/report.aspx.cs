using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AddUser
{
	partial class report : Page
	{
		protected Image Image1;
		protected Image Image2;

		protected void Page_Load(object sender, EventArgs e) 
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");

			string str = Session["strStatus"].ToString();
			string Code = Session["Code"].ToString();
			string DogN = Session["DogN"].ToString();
			string Name = Session["Name"].ToString();
			string ShortName = Session["ShortName"].ToString();
			string Login = Session["Login"].ToString();
			string Password = Session["Password"].ToString();
			bool IsRegister = Convert.ToBoolean(Session["Register"]);
			string Tariff = Session["Tariff"].ToString();
			ChPassMessLB.Visible = !IsRegister;
			RepLb.Visible = !IsRegister;
			if (str != "Yes")
			{
				Server.Transfer("default.aspx");
				return;
			}

			LBClient.Text = Name;
			LBCCard.Text = Name;
			LBShortName.Text = ShortName;
			LBLogin.Text = Login;
			LBLcard.Text = Login;
			LBPassword.Text = Password;
			LBCode.Text = Code;
			LBCard.Text = Code;
			DogNLB.Text = DogN;
			DogNNLB.Text = DogN;
			TariffLB.Text = Tariff;
			TariffD.Text = Tariff;
			LBDate.Text = DateTime.Now.ToString();
			RegDate.Text = DateTime.Now.ToString();

			if (LBClient.Text == ""
				&& LBLogin.Text == ""
				&& LBPassword.Text == "")
				Server.Transfer("default.aspx");
		}
	}
}