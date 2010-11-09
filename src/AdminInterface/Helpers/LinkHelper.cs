using System;
using Castle.MonoRail.Framework;

namespace AdminInterface.Helpers
{
	public class LinkHelper
	{
		public static string Address(string siteroot, uint addressId, string text)
		{
			return String.Format("<a href=\"{0}/deliveries/{1}/edit\">{2}</a>", siteroot, addressId, text);
		}

		public static string User(string siteroot, string login, string text)
		{
			return String.Format("<a href=\"{0}/users/{1}/edit\">{2}</a>", siteroot, login, text);
		}

		public static string Client(string siteroot, uint clientId, string text)
		{
			return String.Format("<a href=\"{0}/client/{1}\">{2}</a>", siteroot, clientId, text);
		}

		/// <summary>
		/// Возвращает виртуальную директорию приложения. БЕЗ '/' в конце
		/// Пример: '/FutureAdm'
		/// </summary>
		/// <param name="context"></param>
		public static string GetVirtualDir(IEngineContext context)
		{
			var virtualDir = context.UrlInfo.AppVirtualDir;
			if (!virtualDir.StartsWith("/"))
				virtualDir = "/" + virtualDir;
			if (virtualDir.EndsWith("/"))
				virtualDir = virtualDir.Remove(virtualDir.Length - 1, 1);
			return virtualDir;
		}
	}
}
