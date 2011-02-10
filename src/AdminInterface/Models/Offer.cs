using System.Collections.Generic;
using Castle.ActiveRecord;
using Common.Web.Ui.Helpers;
using NHibernate.Transform;

namespace AdminInterface.Models
{
	public class Offer
	{
		public string Synonym { get; set; }
		public string CatalogName { get; set; }
		public decimal Cost { get; set; }

		public static IList<Offer> Search(User user, string text)
		{
			IList<Offer> offers = null;
			using (new TransactionScope())
			{
				ArHelper.WithSession(s => {
					s.CreateSQLQuery(@"call future.GetActivePrices(:userid)")
						.SetParameter("userid", user.Id)
						.ExecuteUpdate();

					offers = s.CreateSQLQuery(@"
SELECT if(if(round(cc.Cost * ap.UpCost,2) < MinBoundCost, MinBoundCost, round(cc.Cost*ap.UpCost,2))>MaxBoundCost, MaxBoundCost, if(round(cc.Cost * ap.UpCost,2) < MinBoundCost, MinBoundCost, round(cc.Cost * ap.UpCost,2))) as Cost,
	   s.Synonym,
	   cast(concat(cn.Name, ' ', cf.Form, ' ', ifnull(group_concat(distinct pv.Value ORDER BY prop.PropertyName, pv.Value SEPARATOR ', '), '')) as CHAR) as CatalogName
FROM farm.core0 c0
	JOIN Usersettings.ActivePrices ap on c0.PriceCode = ap.PriceCode
	JOIN farm.CoreCosts cc on cc.Core_Id = c0.Id and cc.PC_CostCode = ap.CostCode
	JOIN farm.synonym as s ON s.SynonymCode = c0.SynonymCode	
	JOIN catalogs.Products as p on p.id = c0.productid
	JOIN Catalogs.Catalog as c on p.catalogid = c.id
	JOIN Catalogs.CatalogNames cn on cn.id = c.nameid
	JOIN Catalogs.CatalogForms cf on cf.id = c.formid
	LEFT JOIN Catalogs.ProductProperties pp on pp.ProductId = p.Id
	LEFT JOIN Catalogs.PropertyValues pv on pv.id = pp.PropertyValueId
	LEFT JOIN Catalogs.Properties prop on prop.Id = pv.PropertyId
WHERE s.Synonym like :SearchText
GROUP BY c0.Id
ORDER BY CatalogName")
							.SetParameter("SearchText", "%" + text + "%")
							.SetResultTransformer(Transformers.AliasToBean(typeof(Offer)))
							.List<Offer>();
				});
				return offers;
			}

		}
	}
}
