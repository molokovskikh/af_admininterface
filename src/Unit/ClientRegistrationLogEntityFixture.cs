﻿using System;
using AdminInterface.Models.Logs;
using NUnit.Framework;


namespace AdminInterface.Test
{
	[TestFixture]
	public class ClientRegistrationLogEntityFixture
	{
		[Test]
		public void HaveOnlyNotCommitedUpdates()
		{
/*			var entity = new ClientRegistrationLogEntity
			             	{
			             		LastUncommitedUpdate = null,
								LastUpdateDate= null,
			             	};
			Assert.That(entity.HaveOnlyNotCommitedUpdates(), Is.False);
			entity.LastUncommitedUpdate = DateTime.Now;
			Assert.That(entity.HaveOnlyNotCommitedUpdates(), Is.True);
			entity.LastUpdateDate = DateTime.Now;
			Assert.That(entity.HaveOnlyNotCommitedUpdates(), Is.False);*/
		}
	}
}
