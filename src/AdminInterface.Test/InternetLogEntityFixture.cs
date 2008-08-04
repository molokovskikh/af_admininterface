using System;
using System.Collections.Generic;
using AdminInterface.Models;
using AdminInterface.Models.Logs;
using AdminInterface.Models.Security;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AdminInterface.Test
{
	[TestFixture]
	public class InternetLogEntityFixture
	{
		[Test]
		public void GetBeginByteNubmerTest()
		{
			InternetLogEntity entity = new InternetLogEntity();
			entity.Parameters = "Id=128293788612187500True&RangeStart=1319424";
			Assert.That(entity.GetBeginByteNubmer(), Is.EqualTo(1319424));
			entity.Parameters = "Id=128293788612187500True";
			Assert.That(entity.GetBeginByteNubmer(), Is.EqualTo(0));
		}

		[Test]
		public void t()
		{
			var type = typeof(Pair<bool, UserPermission>[]);
			bool isContainerType = type == typeof(IList<>);

			if (!isContainerType && type.IsGenericType)
			{
				Type[] genericArgs = type.GetGenericArguments();

				Type genType = typeof(ICollection<>).MakeGenericType(genericArgs);

				isContainerType = genType.IsAssignableFrom(type);
			}
		}
	}
}
