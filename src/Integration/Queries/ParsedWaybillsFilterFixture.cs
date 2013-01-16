using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AdminInterface.Queries;
using Common.Web.Ui.Helpers;
using NUnit.Framework;
using Test.Support;

namespace Integration.Queries
{
	[TestFixture]
	public class ParsedWaybillsFilterFixture : IntegrationFixture
	{
		[Test]
		public void FindWaybillsTest()
		{
			//var a = ParsedWaybillsQueryHelper.GetSelectWithFormat(ParsedWaybillsQueryHelper.DocumentBodyFields, "if(Max(db.{0}) is null, null, '*')");
			//var b = ParsedWaybillsQueryHelper.GetSelectWithFormat(ParsedWaybillsQueryHelper.InvoiceHeadersFields, "if(Max(ih.{0}) is null, null, '*')");
			//var table = ParsedWaybillsQueryHelper.GetSelectWithFormat(ParsedWaybillsQueryHelper.DocumentBodyFields, "\n{0} varchar(1) DEFAULT NULL");
			//var itable = ParsedWaybillsQueryHelper.GetSelectWithFormat(ParsedWaybillsQueryHelper.InvoiceHeadersFields, "\n{0} varchar(1) DEFAULT NULL");
			//itable = itable.Replace("Amount", "IAmount").Replace("NdsAmount", "INdsAmount");

			//var cl = ParsedWaybillsQueryHelper.GetSelectWithFormat(ParsedWaybillsQueryHelper.DocumentBodyFields, "[Display(Name = \"\", Order = 0, GroupName = \"CenterClass\")]\n public string {0} {{ get; set; }}\n\n", "");
			//var cli = ParsedWaybillsQueryHelper.GetSelectWithFormat(ParsedWaybillsQueryHelper.InvoiceHeadersFields, "[Display(Name = \"\", Order = 0, GroupName = \"CenterClass\")]\n public string {0} {{ get; set; }}\n\n", "").Replace("Amount", "IAmount").Replace("NdsAmount", "INdsAmount");

			var filter = new ParsedWaybillsFilter();
			filter.Session = session;
			filter.Period = new DatePeriod(DateTime.Now.AddDays(-7), DateTime.Now);
			var result = filter.Find();
		}
	}
}
