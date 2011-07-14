using System;
using System.Collections.Generic;
using System.Reflection;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.MonoRailExtentions
{
	public class Controller : SmartDispatcherController
	{
		public Controller()
		{
			BeforeAction += (action, context, controller, controllerContext) => {
				controllerContext.PropertyBag["admin"] = Administrator;
			};
		}

/*		protected override object[] BuildMethodArguments(ParameterInfo[] parameters, IRequest request, IDictionary<string, object> actionArgs)
		{
			var args = new object[parameters.Length];
			var paramName = String.Empty;
			var value = String.Empty;

			try
			{
				for(var argIndex = 0; argIndex < args.Length; argIndex++)
				{
					//
					// If the parameter is decorated with an attribute
					// that implements IParameterBinder, it's up to it
					// to convert itself
					//

					var param = parameters[argIndex];
					paramName = GetRequestParameterName(param);

					var handled = false;

					var attributes = param.GetCustomAttributes(false);

					foreach(var attr in attributes)
					{
						var paramBinder = attr as IParameterBinder;

						if (paramBinder != null)
						{
							args[argIndex] = paramBinder.Bind(Context, this, ControllerContext, param);
							handled = true;
							break;
						}
					}

					//
					// Otherwise we handle it
					//

					if (!handled)
					{
						object convertedVal= null;
						var conversionSucceeded = false;

						if (actionArgs != null && actionArgs.ContainsKey(paramName))
						{
							var actionArg = actionArgs[paramName];

							var actionArgType = actionArg != null ? actionArg.GetType() : param.ParameterType;

							convertedVal = Binder.Converter.Convert(param.ParameterType, actionArgType, actionArg, out conversionSucceeded);
						}

						if (!conversionSucceeded)
						{
							convertedVal = Binder.BindParameter(param.ParameterType, paramName, Request.ParamsNode);
						}

						if (convertedVal == null)
						{
							var validatorAccessor = this as IValidatorAccessor;

							if (validatorAccessor != null)
								Binder.Validator = validatorAccessor.Validator;

							var node = Context.Request.ObtainParamsNode(ParamStore.Params);

							var instance = Binder.BindObject(param.ParameterType, "", null, null, node);

							if (validatorAccessor != null)
							{
								validatorAccessor.PopulateValidatorErrorSummary(instance, Binder.GetValidationSummary(instance));
								validatorAccessor.BoundInstanceErrors[instance] = Binder.ErrorList;
							}

							convertedVal = instance;
						}

						args[argIndex] = convertedVal;
					}
				}
			}
			catch(FormatException ex)
			{
				throw new MonoRailException(
					String.Format("Could not convert {0} to request type. " +
					              "Argument value is '{1}'", paramName, Params.Get(paramName)), ex);
			}
			catch(Exception ex)
			{
				throw new MonoRailException(
					String.Format("Error building method arguments. " +
					              "Last param analyzed was {0} with value '{1}'", paramName, value), ex);
			}

			return args;
		}*/

		protected Administrator Administrator
		{
			get
			{
				return SecurityContext.Administrator;
			}
		}
	}

	public class ARController : ARSmartDispatcherController
	{
		public ARController()
		{
			BeforeAction += (action, context, controller, controllerContext) => {
				controllerContext.PropertyBag["admin"] = Administrator;
			};
		}

		protected Administrator Administrator
		{
			get
			{
				return SecurityContext.Administrator;
			}
		}
	}
}