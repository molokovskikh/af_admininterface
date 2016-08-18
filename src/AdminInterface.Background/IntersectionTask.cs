using AdminInterface.Models;
using Common.Web.Ui.Helpers;

namespace AdminInterface.Background
{
	public class IntersectionTask : Task
	{
		protected override void Process()
		{
			Maintainer.MaintainIntersection(Session, "", _ => {});
		}
	}
}