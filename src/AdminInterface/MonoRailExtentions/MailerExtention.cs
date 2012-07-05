using AdminInterface.Mailers;
using AdminInterface.Models;
using Castle.MonoRail.Framework;
using Common.Web.Ui.MonoRailExtentions;

namespace AdminInterface.MonoRailExtentions
{
	public static class MailerExtention
	{
		public static MonorailMailer Mailer(this SmartDispatcherController controller)
		{
			return controller.Mailer<MonorailMailer>();
		}
	}
}