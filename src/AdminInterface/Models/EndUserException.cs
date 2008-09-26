using System;

namespace AdminInterface.Models
{
	public class EndUserException : Exception
	{
		public EndUserException(string message) : base(message)
		{
		}
	}
}
