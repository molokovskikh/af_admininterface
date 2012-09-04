using System;
using NUnit.Framework;
using log4net;

namespace Unit
{
	[TestFixture]
	public class LogTest
	{
		private ILog log;

		[SetUp]
		public void Setup()
		{
			log4net.Config.XmlConfigurator.Configure();
			log = log4net.LogManager.GetLogger("Common.Web.Ui.Helpers.HttpSessionLog");
		}

		[Test(Description = "проверка того, будет ли записан в таблицу Logs сведения о незначительном исключении")]
		public void MySqlAdoNetAppenderTest()
		{
			log.Warn("Это тест. Время : " + DateTime.Now);
		}

		[Test(Description = "Проверка правильности написания логов в зависимости от уровня исключения")]
		public void Test()
		{
			log.Warn("Это тестовая ошибка 1. Уровень исключения WARN. Запись только в базу данных.");
			log.Error("Это тестовая ошибка 2. Уровень исключения ERROR. Запись в лог-файл и в базу данных.");
		}
	}
}
