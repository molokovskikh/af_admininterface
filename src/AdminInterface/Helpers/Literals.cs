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
		[Description("������������ ����������")] UpdateCumulative = 2,
		[Description("������� ����������")] UpdateNormal = 1,
		[Description("������ ���������� ������")] UpdateError = 6,
		[Description("�������")] UpdateBan = 5,
		[Description("��������� ������������")] LimitedCumulative = 18
	}
}