using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace AdminInterface.Models.Logs
{
	public enum LogOperation
	{
		[Description("Вставка")] Insert = 0,
		[Description("Изменение")] Update = 1,
		[Description("Удаление")] Delete = 2,
	}
}
