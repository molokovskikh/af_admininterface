using System;
using System.Net;
using AdminInterface.Models.Billing;
using log4net;

namespace AdminInterface.Helpers
{
	public interface IRequestRunner
	{
		void Request(string uri, string user, string password, string request);
	}

	public class StubRequestRunner : IRequestRunner
	{
		public void Request(string uri, string user, string password, string request)
		{
		}
	}

	public class WebRequestRunner : IRequestRunner
	{
		private ILog log = LogManager.GetLogger(typeof(WebRequestRunner));

		public void Request(string uri, string user, string password, string request)
		{
			var requestUri = uri + "?" + request;
			try {
				var webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
				if (!String.IsNullOrEmpty(user))
					webRequest.Credentials = new NetworkCredential(user, password);
				webRequest.Method = "GET";
				using (var response = (HttpWebResponse)webRequest.GetResponse()) {
					if (response.StatusCode == HttpStatusCode.OK) {
						log.WarnFormat("Выполнения запроса {0} закончилось с неожиданным кодом {1}", requestUri, response.StatusCode);
					}
				}
			}
			catch (Exception e) {
				log.Error(String.Format("Выполнения запроса {0} завершилось с ошибкой", requestUri), e);
			}
		}
	}

	public class BaseRemoteRequest
	{
		public static IRequestRunner Runner;

		protected static void MakeRequest(string uri, string user, string password, string request)
		{
			Runner.Request(uri, user, password, request);
		}
	}
}