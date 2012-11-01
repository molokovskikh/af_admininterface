using System;
using System.Reflection;
using Castle.ActiveRecord.Framework.Internal;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.MonoRailExtentions
{
	public class RecreateOnlyIfNullBinder : ARDataBinder
	{
		private static MethodInfo obtainPrimaryKeyValue;
		private static MethodInfo isValidKey;

		static RecreateOnlyIfNullBinder()
		{
			obtainPrimaryKeyValue = typeof(ARDataBinder).GetMethod("ObtainPrimaryKeyValue", BindingFlags.NonPublic | BindingFlags.Instance);
			isValidKey = typeof(ARDataBinder).GetMethod("IsValidKey", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		protected override bool ShouldRecreateInstance(object value, System.Type type, string prefix, Castle.Components.Binder.Node node)
		{
			if (value == null)
				return true;

			//нужно пересоздавать только объекты у которыех есть id те те объекты которые нужно загрузить из базы
			if (node != null && CurrentARModel != null) {
				if (IsBelongsToRef(CurrentARModel, prefix)) {
					var model = ActiveRecordModel.GetModel(type);
					PrimaryKeyModel pkModel = null;
					var id = obtainPrimaryKeyValue.Invoke(this, new object[] { model, node, prefix, pkModel });
					return (bool)isValidKey.Invoke(this, new[] { id });
				}
			}
			return false;
		}

		public static void Prepare(SmartDispatcherController controller, string expect = null)
		{
			var binder = new RecreateOnlyIfNullBinder();
			binder.AutoLoad = AutoLoadBehavior.NewInstanceIfInvalidKey;
			if (!string.IsNullOrEmpty(expect))
				typeof(ARDataBinder).GetField("expectCollPropertiesList", BindingFlags.Instance | BindingFlags.NonPublic)
					.SetValue(binder, new[] { "root." + expect });

			typeof(SmartDispatcherController)
				.GetField("binder", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(controller, binder);
		}
	}
}