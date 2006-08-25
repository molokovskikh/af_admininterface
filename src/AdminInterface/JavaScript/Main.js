function SetClass(control, className)
{
	i = 0;
	for (i = 0; i < control.children.length; i++)
		if (control.children.item(i).tagName == 'TD')
			control.children.item(i).className = className;
}

function ValidateSearch(source, args)
{
	if (document.getElementById("FindRB_1").checked)
		reg = new RegExp("^\\d{1,10}$");
	if (document.getElementById("FindRB_2").checked)
		reg = new RegExp("^\\d{1,10}$");	
	if (document.getElementById("FindRB_0").checked)
		reg = new RegExp("^.+$");	
	if (document.getElementById("FindRB_3").checked)
		reg = new RegExp("^.+$");		
	if (reg.test(args.Value))
		args.IsValid = true;
	else 
		args.IsValid = false;
}

function ValidateLogin(source, args)
{
	if (document.getElementById("IncludeCB").checked)
	{
		if (document.getElementById("IncludeType").children.item(document.getElementById("IncludeType").selectedIndex).text != "Базовый")
			args.IsValid = args.Value.length > 0;
		else
			args.IsValid = true;
	}
	else
	{
		args.IsValid = args.Value.length > 0;
	}
}