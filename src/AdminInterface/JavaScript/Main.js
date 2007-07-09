function SetClass(control, className)
{
	i = 0;
	for (i = 0; i < control.children.length; i++)
		if (control.children.item(i).tagName == 'TD')
			control.children.item(i).className = className;
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