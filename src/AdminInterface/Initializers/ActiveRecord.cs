using System;
using System.Reflection;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework;
using Common.Tools;
using NHibernate.Mapping;
using NHibernate.Properties;

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
		}

		private bool IsNullableType(Type type)
		{
			if (!type.IsValueType)
				return true;

			if (type.IsNullable())
				return true;

			return false;
		}
	}
}