using System;
using System.Data;
using AdminInterface.Models.Billing;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace AdminInterface.NHibernateExtentions
{
	public class PeriodUserType : IUserType
	{
		public bool Equals(object x, object y)
		{
			return Object.Equals(x, y);
		}

		public int GetHashCode(object x)
		{
			return x.GetHashCode();
		}

		public object NullSafeGet(IDataReader rs, string[] names, object owner)
		{
			var value = NHibernateUtil.String.NullSafeGet(rs, names);
			if (value == null)
				return null;
			return new Period((string) value);
		}

		public void NullSafeSet(IDbCommand cmd, object value, int index)
		{
			if(value == null)
			{
				((IDataParameter) cmd.Parameters[index]).Value = DBNull.Value;
			}
			else
			{
				var ip = (Period) value;
				((IDataParameter)cmd.Parameters[index]).Value = ip.ToSqlString();
			}
		}

		public object DeepCopy(object value)
		{
			return ((ICloneable)value).Clone();
		}

		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		public object Assemble(object cached, object owner)
		{
			return cached;
		}

		public object Disassemble(object value)
		{
			return value;
		}

		public SqlType[] SqlTypes
		{
			get { return new[] {NHibernateUtil.Character.SqlType}; }
		}

		public Type ReturnedType
		{
			get { return typeof(Period); }
		}

		public bool IsMutable
		{
			get { return false; }
		}
	}
}