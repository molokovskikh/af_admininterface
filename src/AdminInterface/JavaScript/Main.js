var havePrototype = true;
try
{
	Prototype.Version;
}
catch(er)
{
	havePrototype = false;
}


if (havePrototype)
{
	document.observe("dom:loaded", function() {
		$$('.HighLightCurrentRow').each(function(table) {
			join(table);
		});
	});
}

function join(control)
{
	control.select('tr').each(function(row){
	
		row.observe('mouseout', function() { 
				row.childElements().each(function(cell)	{
					if (cell.tagName == 'TD')
						cell.removeClassName('SelectedRow')
				});
		});
		
		row.observe('mouseover', function() {
				row.childElements().each(function(cell)	{
					if (cell.tagName == 'TD')
						cell.addClassName('SelectedRow');
				});
		});
	});
}

function ValidateLogin(source, args)
{
	if (document.getElementById("IncludeCB").checked)
	{
		if (document.getElementById("IncludeType") != null)
		{
			if (document.getElementById("IncludeType").children.item(document.getElementById("IncludeType").selectedIndex).text != "Базовый")
				args.IsValid = args.Value.length > 0;
			else
				args.IsValid = true;
		}
	}
	else
	{
		args.IsValid = args.Value.length > 0;
	}
}

function ValidateParent(source, args)
{
	if (args.Value == null || args.Value == "")
		args.IsValid = false;
}

function ShowHiden(sender)
{
	$$(".ShowHiden").first().className = "HideVisible";
	sender.onclick = function() { HideVisible(sender); } 
	$$(".HidenFolder").first().className = "VisibleFolder";
}

function HideVisible(sender)
{
	$$(".HideVisible").first().className = "ShowHiden";
	sender.onclick = function() { ShowHiden(sender); } 
	$$(".VisibleFolder").first().className = "HidenFolder";
}

function SetupCalendarElements()
{
  $$(".CalendarInput")
	.each(function(value, index)
			{
				value.id = "CalendarInput" + index;
				Calendar.setup({
					ifFormat: "%d.%m.%Y",
					inputField: value.id,
					weekNumbers: false,
					showOthers: true
				})
			});
}