using System.Collections;
using System.Collections.Generic;
using Castle.MonoRail.Framework.Services;

namespace AdminInterface.MonoRailExtentions
{
	public interface IUrlContributor
	{
		IDictionary GetQueryString();
	}

	public class UrlBuilder : DefaultUrlBuilder
	{
		protected override void AppendQueryString(UrlParts parts, UrlBuilderParameters parameters)
		{
			var contributor = parameters.QueryString as IUrlContributor;
			if (contributor != null)
				parameters.QueryString = contributor.GetQueryString();

			base.AppendQueryString(parts, parameters);
		}
	}
}