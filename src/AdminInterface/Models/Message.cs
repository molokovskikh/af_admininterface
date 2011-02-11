using System;

namespace AdminInterface.Models
{
	public class Message
	{
		public Message(string message, bool isError)
		{
			MessageText = message;
			IsError = isError;
		}

		public Message(string message)
		{
			MessageText = message;
		}

		public string MessageText { get; private set; }

		public bool IsError { get; private set; }

		public string GetClass()
		{
			if (IsError)
				return "err";
			return "notice";
		}

		public override string ToString()
		{
			return MessageText;
		}

		public static Message Error(string message)
		{
			return new Message(message, true);
		}

		public static Message Notify(string message)
		{
			return new Message(message, false);
		}
	}
}
