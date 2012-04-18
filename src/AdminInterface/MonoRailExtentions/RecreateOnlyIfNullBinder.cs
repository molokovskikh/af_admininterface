using System.Reflection;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.MonoRailExtentions
{
	public class RecreateOnlyIfNullBinder : ARDataBinder
	{
		protected override bool ShouldRecreateInstance(object value, System.Type type, string prefix, Castle.Components.Binder.Node node)
		{
			return value == null;
		}

		public static void Prepare(SmartDispatcherController controller, string expect = null)
		{
			var binder = new RecreateOnlyIfNullBinder();
			binder.AutoLoad = AutoLoadBehavior.NewInstanceIfInvalidKey;
			if (!string.IsNullOrEmpty(expect))
				typeof(ARDataBinder).GetField("expectCollPropertiesList", BindingFlags.Instance | BindingFlags.NonPublic)
					.SetValue(binder, new [] { "root." + expect });

			typeof (SmartDispatcherController)
				.GetField("binder", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(controller, binder);
		}
	}
}