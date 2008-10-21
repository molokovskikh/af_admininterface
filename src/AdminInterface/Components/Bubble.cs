using Castle.MonoRail.Framework;

namespace AdminInterface.Components
{
	public class Bubble : ViewComponent
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
