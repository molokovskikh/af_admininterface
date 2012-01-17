using System;
using System.Collections.Generic;
using System.Reflection;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using Common.Web.Ui.Models;
using NHibernate;
using NHibernate.Dialect.Function;
using NHibernate.Engine;
using NHibernate.Mapping;
using NHibernate.Properties;
using NHibernate.Type;

namespace AdminInterface.Initializers
{
	public class ActiveRecord
	{
		public void Initialize(IConfigurationSource config)
		{
			ActiveRecordStarter.Initialize(new[] {
					Assembly.Load("AdminInterface"),
					Assembly.Load("Common.Web.Ui")
				},
				config);
			var configuration = ActiveRecordMediator.GetSessionFactoryHolder().GetAllConfigurations()[0];
			configuration.AddSqlFunction("DATE_ADD", new StandardSQLFunction("DATE_ADD"));
			configuration.AddSqlFunction("group_concat", new StandardSQLFunction("group_concat"));
			foreach(var clazz in configuration.ClassMappings)
			{
				//тут баг для nested объектов я не выставлю is null
				foreach(var property in clazz.PropertyIterator)
				{
					var getter = (IGetter)property.GetGetter(clazz.MappedClass);
					var type = getter.ReturnType;
					foreach (Column column in property.ColumnIterator)
					{
						//var type = ((SimpleValue)column.Value).Type.ReturnedClass;
						if (String.IsNullOrEmpty(column.DefaultValue)
							&& column.IsNullable
							&& !IsNullableType(type))
						{
							column.IsNullable = false;
							column.DefaultValue = "0";
						}
					}
				}
			}

			SetupSecurityFilters();
		}

		private bool IsNullableType(Type type)
		{
			if (!type.IsValueType)
				return true;

			if (type.IsNullable())
				return true;

			return false;
		}

		private void SetupSecurityFilters()
		{
			var configuration = ActiveRecordMediator.GetSessionFactoryHolder()
				.GetAllConfigurations()[0];

			configuration.FilterDefinitions.Add("RegionFilter",
				new FilterDefinition("RegionFilter",
					"",
					new Dictionary<string, IType> {{"AdminRegionMask", NHibernateUtil.UInt64}},
					true));
			configuration.FilterDefinitions.Add("DrugstoreOnlyFilter",
				new FilterDefinition("DrugstoreOnlyFilter", "", new Dictionary<string, IType>(), true));
			configuration.FilterDefinitions.Add("SupplierOnlyFilter",
				new FilterDefinition("SupplierOnlyFilter", "", new Dictionary<string, IType>(), true));

			var regionMapping = configuration.GetClassMapping(typeof (Region));
			regionMapping.AddFilter("RegionFilter", "if(RegionCode is null, 1, RegionCode & :AdminRegionMask > 0)");
		}
	}
}