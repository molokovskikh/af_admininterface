using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AdminInterface.ViewModels
{
	public class SelectItemList
	{
		public string[] Values { get; set; }
		public string Value { get; set; }

		public List<string> SelectedItems
		{
			get
			{
				if (Values != null && Values.Length != 0) {
					return Values.ToList();
				}
				if (!string.IsNullOrEmpty(Value)) {
					return Value.Split(',').ToList();
				}
				return new List<string>();
			}
		}

		public IEnumerable<SelectListItem> ItemsList { get; set; }
	}
}