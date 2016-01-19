using System;
using System.Net;
using System.Text;
using Castle.ActiveRecord.Framework.Internal;
using Newtonsoft.Json;

namespace AdminInterface.Models
{
	public class Redmine
	{
		public static Action<string, string> DebugCallback;

		public static void CreateIssue(string url, string subject, string body, string assignedTo = null)
		{
			if (DebugCallback != null) {
				DebugCallback(subject, body);
				return;
			}

			if (String.IsNullOrEmpty(url))
				return;
			var data = JsonConvert.SerializeObject(new {
				issue = new {
					subject = subject,
					description = body,
					assigned_to_id = assignedTo
				}
			});
			var webClient = new WebClient();
			webClient.Encoding = Encoding.UTF8;
			webClient.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			webClient.UploadString(url, data);
		}
	}
}