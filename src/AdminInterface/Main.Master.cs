using System;
using System.Web;
using System.Web.UI;
using AdminInterface.Helpers;
using MySql.Data.MySqlClient;

namespace AdminInterface
{
	public partial class Main : MasterPage
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SiteMapPath.Provider.SiteMapResolve += SiteMapResolve;
		}

		private SiteMapNode SiteMapResolve(object sender, SiteMapResolveEventArgs e)
		{
			var currentNode = e.Provider.CurrentNode.Clone(true);
			if (currentNode.Url.EndsWith("/managep.aspx"))
				currentNode.ParentNode.Url += e.Context.Request["cc"];
			else if (currentNode.Url.EndsWith("/SenderProperties.aspx")) {
				uint firmCode;
				using (var connection = new MySqlConnection(Literals.GetConnectionString())) {
					connection.Open();
					var command = new MySqlCommand(@"
select firmcode 
from ordersendrules.order_send_rules osr
where osr.id = ?ruleId
", connection);
					command.Parameters.AddWithValue("?RuleId", e.Context.Request["RuleId"]);
					firmCode = Convert.ToUInt32(command.ExecuteScalar());
				}
				currentNode.ParentNode.Url += "?cc=" + firmCode;
				currentNode.ParentNode.ParentNode.Url += firmCode;
			}
			else if (currentNode.Url.EndsWith("/EditRegionalInfo.aspx")) {
				uint firmCode;
				using (var connection = new MySqlConnection(Literals.GetConnectionString())) {
					connection.Open();
					var command = new MySqlCommand(@"
SELECT FirmCode
FROM usersettings.regionaldata rd
WHERE RowID = ?Id", connection);
					command.Parameters.AddWithValue("?Id", Convert.ToUInt32(e.Context.Request["id"]));
					firmCode = Convert.ToUInt32(command.ExecuteScalar());
				}
				currentNode.ParentNode.Url += "?cc=" + firmCode;
				currentNode.ParentNode.ParentNode.Url += firmCode;
			}
			else if (currentNode.Url.EndsWith("/managecosts.aspx")) {
				uint firmCode;
				using (var connection = new MySqlConnection(Literals.GetConnectionString())) {
					connection.Open();
					var command = new MySqlCommand(@"
SELECT FirmCode
FROM usersettings.PricesData pd
WHERE PriceCode = ?Id", connection);
					command.Parameters.AddWithValue("?Id", Convert.ToUInt32(e.Context.Request["pc"]));
					firmCode = Convert.ToUInt32(command.ExecuteScalar());
				}
				currentNode.ParentNode.Url += "?cc=" + firmCode;
				currentNode.ParentNode.ParentNode.Url += firmCode;
			}
			return currentNode;
		}
	}
}