function SetClass(control, className)
{
	i = 0;
	for (i = 0; i < control.children.length; i++)
		if (control.children.item(i).tagName == 'TD')
			control.children.item(i).className = className;
}