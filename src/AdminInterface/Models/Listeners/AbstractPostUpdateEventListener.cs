using System;
using System.Linq;
using NHibernate.Persister.Entity;

namespace AdminInterface.Models.Listeners
{
	public class AbstractPostUpdateEventListener
	{
		protected bool PropertyDirty(IEntityPersister persister, int[] dirty, string[] properties)
		{
			if (dirty == null)
				return false;

			foreach (var dirtyIndex in dirty) {
				var property = persister.PropertyNames[dirtyIndex];
				if (properties.Any(s => s.Equals(property, StringComparison.OrdinalIgnoreCase)))
					return true;
			}

			return false;
		}
	}
}