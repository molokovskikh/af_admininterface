namespace AdminInterface.Models.Security
{
	/// <summary>
	/// суть в том что т.к. каждая модель которой нужно сделать проверку прав доступа может реализовать по
	/// своему для этого и нужен этот интерфейс
	/// </summary>
	public interface ISecurityAware
	{
		void DoSecurityCheck();
	}
}