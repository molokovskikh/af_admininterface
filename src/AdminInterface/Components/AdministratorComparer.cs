using AdminInterface.Models.Security;
using System.Collections.Generic;

namespace AdminInterface.Components
{
	public class AdministratorComparer : IEqualityComparer<Administrator>
	{
		public bool Equals(Administrator x, Administrator y)
		{
			if (x.PhoneSupportFormat == y.PhoneSupportFormat) {
				return true;
			}
			else {
				return false;
			}
		}

		public int GetHashCode(Administrator x)
		{
			return 0;
		}
	}
}