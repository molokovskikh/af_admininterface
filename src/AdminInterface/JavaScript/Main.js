var havePrototype = true;
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

function ShowHidden(sender)
{
	if ($$(".ShowHiden").length > 1 || $$(".hidden").length > 1) {
		jQuery(".ShowHiden[title=\"" + sender.title + "\"]")[0].className = "HideVisible";        
		jQuery(".hidden[title=\"" + sender.title + "\"]")[0].className = "VisibleFolder";	
	}
	else {
		$$(".ShowHiden").first().className = "HideVisible";
		$$(".hidden").first().className = "VisibleFolder";
	}
	sender.onclick = function() { HideVisible(sender); }
}

function HideVisible(sender)
{
	if ($$(".HideVisible").length > 1 || $$(".VisibleFolder").length > 1) {
		jQuery(".HideVisible[title=\"" + sender.title + "\"]")[0].className = "ShowHiden";
		jQuery(".VisibleFolder[title=\"" + sender.title + "\"]")[0].className = "hidden";
	}
	else {
		$$(".HideVisible").first().className = "ShowHiden";
		$$(".VisibleFolder").first().className = "hidden";
	}
	sender.onclick = function() { ShowHidden(sender); } 
}

function SetupCalendarElements()
{
	$$(".CalendarInput")
		.each(function(value, index) {
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

function ShowElement(show, selector) {
	var displayValue = "none";
	if (show) {
		displayValue = "block";
	}
	jQuery(selector).css("display", displayValue);
}