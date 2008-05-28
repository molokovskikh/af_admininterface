using System.Reflection;
using AdminInterface.Models;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Config;
using log4net.Config;
using NHibernate;
using NHibernate.Dialect.Function;
using NUnit.Framework;

namespace AdminInterface.Test.Models
{
	[TestFixture]
	public class BillingSearchItemFixture
	{
		public BillingSearchItemFixture()
		{
			XmlConfigurator.Configure();
			ActiveRecordStarter.Initialize(new[]
			                               	{
			                               		Assembly.Load("AdminInterface"),
			                               		Assembly.Load("Common.Web.Ui")
			                               	},
			                               ActiveRecordSectionHandler.Instance);
			var functions = ActiveRecordMediator
				.GetSessionFactoryHolder()
				.GetAllConfigurations()[0]
				.SqlFunctions;
			functions.Add("if", new SQLFunctionTemplate(null, "if(?1, ?2, ?3"));
			functions.Add("group_concat", new SQLFunctionTemplate(NHibernateUtil.String, "group_concat(?1"));
		}

		[Test]
		public void FindBy2()
		{
			var properties = new BillingSearchProperties
			                 	{
			                 		SearchText = "тест",
									PayerState = PayerStateFilter.Debitors,
			                 	};
			BillingSearchItem.FindBy2(properties);
		}

		[Test]
		public void FindBy_billingCode()
		{
			var properties = new BillingSearchProperties
			                 	{
			                 		SearchText = "13",
			                 		PayerState = PayerStateFilter.Debitors,
			                 		SearchBy = SearchBy.BillingCode,
			                 	};
			BillingSearchItem.FindBy2(properties);
		}

		[Test]
		public void FindBy_code()
		{
			var properties = new BillingSearchProperties
			                 	{
			                 		SearchText = "13",
			                 		PayerState = PayerStateFilter.Debitors,
			                 		SearchBy = SearchBy.Code,
			                 	};
			BillingSearchItem.FindBy2(properties);
		}

		[Test]
		public void FindBy_name()
		{
			var properties = new BillingSearchProperties
			{
				SearchText = "тест",
				PayerState = PayerStateFilter.Debitors,
				SearchBy = SearchBy.Name,
			};
			BillingSearchItem.FindBy2(properties);
		}
	}
}
