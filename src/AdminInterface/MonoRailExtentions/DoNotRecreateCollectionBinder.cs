using System.Reflection;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.MonoRailExtentions
{
	public class DoNotRecreateCollectionBinder : ARDataBinder
	{
		protected override bool ShouldRecreateInstance(object value, System.Type type, string prefix, Castle.Components.Binder.Node node)
		{
			return value == null;
		}

		public static void Prepare(SmartDispatcherController controller, string expect)
		{
			var binder = new DoNotRecreateCollectionBinder();
			binder.AutoLoad = AutoLoadBehavior.NewInstanceIfInvalidKey;
			typeof(ARDataBinder).GetField("expectCollPropertiesList", BindingFlags.Instance | BindingFlags.NonPublic)
				.SetValue(binder, new [] { "root." + expect });

			typeof (SmartDispatcherController)
				.GetField("binder", BindingFlags.NonPublic | BindingFlags.Instance)
				.SetValue(controller, binder);
		}
	}
}