using AdminInterface.Models;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[Layout("NewDefault")]
	public class DeliveriesController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			PropertyBag["client"] = Client.FindAndCheck(clientId);
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("delivery")] Address address, uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			address.Client = client;
			address.Save();
			Flash["Message"] = new Message("Адрес доставки создан");
			RedirectUsingRoute("client", "info", new { cc = client.Id });
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(uint id)
		{
			PropertyBag["delivery"] = Address.Find(id);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("delivery", AutoLoadBehavior.Always)] Address address)
		{
			address.Update();
			Flash["Message"] = new Message("Сохранено");
			RedirectUsingRoute("client", "info", new { cc = address.Client.Id });
		}
	}
}
