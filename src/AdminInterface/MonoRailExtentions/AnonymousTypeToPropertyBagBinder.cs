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
			foreach (var property in type.GetProperties(BindingFlags.Public
			                                            | BindingFlags.Instance
			                                            | BindingFlags.GetProperty))
				controllerContext.PropertyBag[property.Name] = property.GetValue(returnValue, null);
		}
	}
}
