#undef DEBUG
using System;
using System.Web;
using System.Web.SessionState;

namespace AddUser
{
	public class Global : System.Web.HttpApplication
	{
		private System.ComponentModel.IContainer components;
		
		public Global()
		{
			InitializeComponent();
		}
		
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}

		void Application_Start(object sender, EventArgs e)
		{
		}

		void Session_Start(object sender, EventArgs e)
		{
			Session["strStatus"] = "No";
			Session["strError"] = "";
			string UserName;
			UserName = HttpContext.Current.User.Identity.Name;
			if (UserName.Substring(0, 7) == "ANALIT\\")
			#if DEBUG
				UserName = "morozov";
			#else
				UserName = UserName.Substring(7);
			#endif
			Session["UserName"] = UserName;
			Session["SessionID"] = this.Session.SessionID;
		}

		void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		void Application_Error(object sender, EventArgs e)
		{
		}

		void Session_End(object sender, EventArgs e)
		{
			Response.Cookies["Inforoom.Admins.ShowStatsC"].Value = Session["MaxID"].ToString();
			Response.Cookies["Inforoom.Admins.ShowStatsC"].Expires = DateTime.Now.AddYears(2);
		}

		void Application_End(object sender, EventArgs e)
		{
		}
	}
}