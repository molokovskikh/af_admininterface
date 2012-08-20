using Castle.ActiveRecord;

namespace AdminInterface.Models
{
	[ActiveRecord(Schema = "catalogs")]
	public class CatalogName
	{
		public CatalogName()
		{
		}

		public CatalogName(string name)
		{
			Name = name;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Name { get; set; }
	}

	[ActiveRecord(Schema = "catalogs")]
	public class CatalogForm
	{
		public CatalogForm()
		{
		}

		public CatalogForm(string name)
		{
			Form = name;
		}

		[PrimaryKey]
		public virtual uint Id { get; set; }

		[Property]
		public virtual string Form { get; set; }
	}
}