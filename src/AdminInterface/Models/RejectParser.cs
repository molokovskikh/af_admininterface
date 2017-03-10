using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using AdminInterface.Models.Suppliers;
using Castle.ActiveRecord;
using Common.Tools;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "Customers")]
	public class RejectParser
	{
		public RejectParser(Supplier supplier)
			: this()
		{
			Supplier = supplier;
		}

		public RejectParser()
		{
			Lines = new List<RejectParserLine>();
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property, DisplayName("Название"), Required]
		public virtual string Name { get; set; }

		[BelongsTo]
		public virtual Supplier Supplier { get; set; }

		[HasMany(Cascade = ManyRelationCascadeEnum.AllDeleteOrphan)]
		public virtual IList<RejectParserLine> Lines { get; set; }

		public List<SelectListItem> Fields(string selected)
		{
			var lineGroup = new SelectListGroup {
				Name = "Строка"
			};
			var items = new List<SelectListItem> {

				//строка
				new SelectListItem {
					Text = "Код товара",
					Value = "Code",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Наименование товара",
					Value = "Product",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Производитель товара",
					Value = "Producer",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Количество заказанных товаров",
					Value = "Ordered",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Количество отказов по товару",
					Value = "Rejected",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Стоимость товара",
					Value = "Cost",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Код производителя, строка макс 255 символов",
					Value = "CodeCr",
					Group = lineGroup,
				},
				new SelectListItem {
					Text = "Номер заявки АналитФАРМАЦИЯ",
					Value = "OrderId",
					Group = lineGroup,
				}
			};
			items.Each(x => x.Selected = x.Value == selected);
			items = items.OrderBy(s => s.Group.Name).ThenBy(s => s.Text).ToList();
			return items;
		}
	}

	[ActiveRecord(Schema = "Customers")]
	public class RejectParserLine
	{
		[PrimaryKey]
		public virtual uint Id { get; set; }

		[BelongsTo]
		public virtual RejectParser Parser { get; set; }

		[Property]
		public virtual string Src { get; set; }

		[Property]
		public virtual string Dst { get; set; }
	}
}