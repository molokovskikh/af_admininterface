using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;

namespace AddUser
{
	partial class report : Page
	{
		protected Image Image1;
		protected Image Image2;

		protected void Page_Load(object sender, EventArgs e) 
		{
			var Code = Session["Code"].ToString();
			var DogN = Session["DogN"].ToString();
			var Name = Session["Name"].ToString();
			var ShortName = Session["ShortName"].ToString();
			var Login = Session["Login"].ToString();
			var Password = Session["Password"].ToString();
			var IsRegister = Convert.ToBoolean(Session["Register"]);
			var Tariff = Session["Tariff"].ToString();
			ChPassMessLB.Visible = !IsRegister;
			RepLb.Visible = !IsRegister;

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