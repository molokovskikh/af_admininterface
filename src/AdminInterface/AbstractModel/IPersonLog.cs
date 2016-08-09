using System;
using AdminInterface.Models;

namespace AdminInterface.AbstractModel
{
	interface IPersonLog
	{
		User User { get; set; }

		DateTime CreatedOn { get; set; }

		string Version { get; set; }

		string RequestToken { get; set; }
	}
}
