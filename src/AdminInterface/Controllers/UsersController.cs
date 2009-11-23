using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.ActiveRecordSupport;
using Castle.MonoRail.Framework;

namespace AdminInterface.Controllers
{
	[Layout("NewDefault")]
	public class UsersController : SmartDispatcherController
	{
		[AccessibleThrough(Verb.Get)]
		public void Add(uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			PropertyBag["client"] = client;
			PropertyBag["permissions"] = UserPermission.FindPermissionsAvailableFor(client);
		}

		[AccessibleThrough(Verb.Post)]
		public void Add([DataBind("user")] User user, uint clientId)
		{
			var client = Client.FindAndCheck(clientId);
			user.Client = client;
			user.Save();
			Flash["Message"] = new Message("Пользователь создан");
			RedirectUsingRoute("client", "info", new { cc = client.Id });
		}

		[AccessibleThrough(Verb.Get)]
		public void Edit(string login)
		{
			var user = User.GetByLogin(login);
			PropertyBag["user"] = user;
			PropertyBag["admin"] = SecurityContext.Administrator;
			PropertyBag["client"] = user.Client;
			PropertyBag["permissions"] = UserPermission.FindPermissionsAvailableFor(user.Client);
		}

		[AccessibleThrough(Verb.Post)]
		public void Update([ARDataBind("user", AutoLoad = AutoLoadBehavior.Always, Expect = "user.AssignedPermissions, user.AvaliableAddresses")] User user)
		{
			user.Update();
			Flash["Message"] = new Message("Сохранен");
			RedirectUsingRoute("client", "info", new { cc = user.Client.Id });
		}
	}
}
