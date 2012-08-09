using System.Collections;
using AddUser;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Common.Web.Ui.Helpers;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.Initializers
{
	public class Route
	{
		public void Initialize()
		{
			var engine = RoutingModuleEx.Engine;

			engine.Add(
				new PatternRoute("/<controller>/<id>")
					.DefaultForAction().Is("show")
					.Restrict("id").ValidInteger
				);

			engine.Add(
				new BugRoute(
					new PatternRoute("/<controller>/[action]")
						.DefaultForAction().Is("index")
					)
				);

			engine.Add(
				new PatternRoute("/<controller>/[id]/<action>")
					.Restrict("id")
					.ValidInteger
				);

			engine.Add(
				new PatternRoute("/client/[id]/[action]")
					.DefaultForController().Is("clients")
					.DefaultForAction().Is("show")
					.Restrict("id").ValidInteger
				);

			engine.Add(
				new PatternRoute("/client/<action>")
					.DefaultForController().Is("clients")
				);

			engine.Add(
				new PatternRoute("/deliveries/[id]/[action]")
					.DefaultForController().Is("addresses")
					.DefaultForAction().Is("edit")
					.Restrict("id").ValidInteger
				);

			engine.Add(
				new PatternRoute("/deliveries/[action]")
					.DefaultForController().Is("addresses")
				);

			engine.Add(new PatternRoute("/client/[clientId]/orders")
				.DefaultForController().Is("Logs")
				.DefaultForAction().Is("Orders")
				.Restrict("clientId").ValidInteger);

			engine.Add(new PatternRoute("/users/search")
				.DefaultForController().Is("UserSearch")
				.DefaultForAction().Is("Search"));

			//для обратной совместимости что бы работали старые ссылки
			engine.Add(new PatternRoute("/UserSearch/SearchBy")
				.DefaultForController().Is("UserSearch")
				.DefaultForAction().Is("Search"));

			engine.Add(new PatternRoute("/")
				.DefaultForController().Is("Main")
				.DefaultForAction().Is("Index"));

			engine.Add(new PatternRoute("default.aspx")
				.DefaultForController().Is("Main")
				.DefaultForAction().Is("Index"));
		}
	}
}