﻿using System;
using System.Linq;
using Castle.ActiveRecord;
using System.Collections.Generic;
using NHibernate.Criterion;

namespace AdminInterface.Models.Logs
{
	public class AuthorizationLogEntityList : List<AuthorizationLogEntity>
	{
		public AuthorizationLogEntityList(List<AuthorizationLogEntity> list)
		{
			this.AddRange(list);
		}

		public AuthorizationLogEntity GetEntityByUserId(uint id)
		{
			AuthorizationLogEntity result = null;
			foreach (var entity in this)
				if(entity.Id == id)
				{
					result = entity;
					break;
				}
			return result;
		}
	}

	[ActiveRecord(Table = "logs.AuthorizationDates")]
	public class AuthorizationLogEntity : ActiveRecordBase<AuthorizationLogEntity>
	{
		public AuthorizationLogEntity(uint id)
		{
			Id = id;
		}

		public AuthorizationLogEntity() {}

		[PrimaryKey("UserId", Generator = PrimaryKeyType.Assigned)]
		public uint Id { get; set; }

		[Property]
		public DateTime? CITime { get; set; }

		[Property]
		public DateTime? AFTime { get; set; }

		[Property]
		public DateTime? AOLTime { get; set; }

		[Property]
		public DateTime? IOLTime { get; set; }

		public static List<AuthorizationLogEntity> GetEntitiesByUsers(List<User> users)
		{
			return FindAll(Expression.In("Id", users.Select(r => r.Id).ToArray())).ToList();
		}
	}
}
