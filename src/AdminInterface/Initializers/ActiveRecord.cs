using System.Collections.Generic;
using Castle.ActiveRecord.Framework;
using Common.Web.Ui.ActiveRecordExtentions;
using Common.Web.Ui.Models;
using Common.Web.Ui.NHibernateExtentions;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;

namespace AdminInterface.Initializers
{
	public class ActiveRecord : ActiveRecordInitializer
	{
		public ActiveRecord()
		{
			Assemblies = new[] { "AdminInterface", "Common.Web.Ui" };
		}

		public override void Initialize(IConfigurationSource config)
		{
			base.Initialize(config);

			SetupSecurityFilters();
		}

		private void SetupSecurityFilters()
		{
			Configuration.FilterDefinitions.Add("RegionFilter",
				new FilterDefinition("RegionFilter",
					"",
					new Dictionary<string, IType> { { "AdminRegionMask", NHibernateUtil.UInt64 } },
					true));
			Configuration.FilterDefinitions.Add("DrugstoreOnlyFilter",
				new FilterDefinition("DrugstoreOnlyFilter", "", new Dictionary<string, IType>(), true));
			Configuration.FilterDefinitions.Add("SupplierOnlyFilter",
				new FilterDefinition("SupplierOnlyFilter", "", new Dictionary<string, IType>(), true));

			var regionMapping = Configuration.GetClassMapping(typeof(Region));
			regionMapping.AddFilter("RegionFilter", "if(RegionCode is null, 1, RegionCode & :AdminRegionMask > 0)");
		}
	}
}