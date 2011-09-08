using Castle.MonoRail.ActiveRecordSupport;

namespace AdminInterface.MonoRailExtentions
{
	//Account абстрактный тип оп этому биндер по умолчанию будет 
	//его игнорировать тк обосновано считает что объект этого типа он создать не может
	//но нам не нужно его создавать достаточно получить его из базы
	public class AccountBinder : ARDataBinder
	{
		protected override bool ShouldIgnoreType(System.Type instanceType)
		{
			if (instanceType.IsAbstract)
				return false;
			return base.ShouldIgnoreType(instanceType);
		}
	}
}