using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using Castle.MonoRail.Framework;

namespace AdminInterface.Components
{
	public class Bredcrumbs : ViewComponent
	{
		public override void Render()
		{
			foreach (var key in ComponentParams.Keys)
			{
				Context.ContextVars[key] = ComponentParams[key];
				Context.ContextVars[key + ".@bubbleUp"] = true;
			}
		}

	}
}
