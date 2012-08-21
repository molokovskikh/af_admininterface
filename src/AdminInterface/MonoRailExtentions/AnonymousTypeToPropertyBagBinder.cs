using System;
using System.Reflection;
using Castle.MonoRail.Framework;

namespace AdminInterface.MonoRailExtentions
{
	[AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
	public class AnonymousTypeToPropertyBagBinder : Attribute, IReturnBinder
	{
		public void Bind(IEngineContext context,
			IController controller,
			IControllerContext controllerContext,
			Type returnType,
			object returnValue)
		{
			if (returnValue == null)
				return;

			var type = returnValue.GetType();
			var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty;
			foreach (var property in type.GetProperties(flags))
				controllerContext.PropertyBag[property.Name] = property.GetValue(returnValue, null);
		}
	}
}