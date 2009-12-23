﻿var havePrototype = true;
try
{
	Prototype.Version;
}
catch(er)
{
	havePrototype = false;
}

var havejQuery = true;
try {
	jQuery;
}
catch (er) {
	havejQuery = false;
}


if (havePrototype)
{
	document.observe("dom:loaded", function() {
		$$('.HighLightCurrentRow').each(function(table) {
			join(table);
		});
		
		$$(".ShowHiden").each(function(element){
			element.onclick = function() { ShowHidden(element); } 
		});
	
		$$(".HideVisible").each(function(element){
			element.onclick = function() { HideVisible(element); } 
		});
	});	
}

function join(control)
{
	control.select('tr').each(function(row){
	
		row.observe('mouseout', 
					function() { 				
						row.removeClassName('SelectedRow');
					});
		
		row.observe('mouseover', 
					function() {
						row.addClassName('SelectedRow');
					});
	});
}

if (havejQuery) {
	$(document).ready(function() {
		$('.HighLightCurrentRow tr').not('.NoHighLightRow').each(function() { jQueryJoin(this) });
	});
}

function jQueryJoin(control) {
	$(control).mouseout(function() { $(this).removeClass('SelectedRow'); });
	$(control).mouseover(function() { $(this).addClass('SelectedRow'); });
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

function ShowHidden(sender)
{
    $$(".ShowHiden").first().className = "HideVisible";
	sender.onclick = function() { HideVisible(sender); } 
	$$(".hidden").first().className = "VisibleFolder";
}

function HideVisible(sender)
{
    $$(".HideVisible").first().className = "ShowHiden";
	sender.onclick = function() { ShowHidden(sender); } 
	$$(".VisibleFolder").first().className = "hidden";
}

function SetupCalendarElements()
{
  $$(".CalendarInput")
	.each(function(value, index)
			{
				value.id = "CalendarInput" + index;
				value.previous().id = "CalendarInputField" + index;
				Calendar.setup({
					ifFormat: "%d.%m.%Y",
					inputField: value.previous().id,
					button: value.id,
					weekNumbers: false,
					showOthers: true
				})
			});
}