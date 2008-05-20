﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AdminInterface
{
	public partial class OfficeUserRegistrationReport : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Convert.ToBoolean(Session["IsLoginCreate"]))
			{
				var label = new Label
				            	{
				            		Text = "Учетная запись в ActiveDirectory уже существует",
				            	};
				Controls.Clear();
				Controls.Add(label);
			}
			else
			{
				LBShortName.Text = Session["FIO"].ToString();
				LBPassword.Text = Session["Password"].ToString();
				LBLogin.Text = Session["Login"].ToString();
				RegDate.Text = DateTime.Now.ToString();
			}
		}
	}
}
