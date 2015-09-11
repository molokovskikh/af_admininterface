using System.Reflection;
using Castle.Components.Binder;
using Castle.MonoRail.ActiveRecordSupport;

namespace AdminInterface.MonoRailExtentions
{
	/// <summary>
	/// в оргинальном биндере SetPropertyValue использует Access что бы обратиться к полям
	/// но для прокси это работать не будет, используем getter и setter
	/// </summary>
	public class ARDataBind2Attribute : ARDataBindAttribute
	{
		public ARDataBind2Attribute(string prefix) : base(prefix)
		{
		}

		public ARDataBind2Attribute(string prefix, AutoLoadBehavior autoLoadBehavior) : base(prefix, autoLoadBehavior)
		{
		}

		protected override IDataBinder CreateBinder()
		{
			return new ARDataBinder2();
		}
	}

	public class ARDataBinder2 : ARDataBinder
	{
		protected override void SetPropertyValue(object instance, PropertyInfo prop, object value)
		{
			if (!prop.CanWrite)
				return;
			prop.SetValue(instance, value, null);
		}
	}
}