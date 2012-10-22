using System.ComponentModel;
using System.Configuration;

namespace AdminInterface.Helpers
{
	public class Literals
	{
		public static string GetConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["Main"].ConnectionString;
		}
	}

	public enum StatisticsType
	{
		[Description("Кумулятивные обновления")] UpdateCumulative = 2,
		[Description("Обычные обновления")] UpdateNormal = 1,
		[Description("Ошибки подготовки данных")] UpdateError = 6,
		[Description("Запреты")] UpdateBan = 5,
		[Description("Частисные комулятивные")] LimitedCumulative = 18
	}
}