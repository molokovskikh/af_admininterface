using Castle.ActiveRecord;
using NHibernate.Expression;

namespace AdminInterface.Models
{
	[ActiveRecord("accessright.regionaladmins")]
	public class Administrator
	{
		[PrimaryKey("RowId")]
		public uint Id { get; set; }

		[Property]
		public virtual bool AlowChangePassword { get; set; }

		[Property]
		public virtual string UserName { get; set; }

		[Property]
		public string Email { get; set; }

		public static Administrator GetByName(string name)
		{
			return ActiveRecordMediator<Administrator>.FindOne(Expression.Eq("UserName", name.Replace("ANALIT\\", "")));
		}
	}
}
