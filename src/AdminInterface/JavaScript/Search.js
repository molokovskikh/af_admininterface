function joinSearchHelper(textBox, titlesAndIds)
{
	textBox.observe("blur", function(event) {
		setSearchTitle(textBox, titlesAndIds);
	});
	
	textBox.observe("focus", function(event) {
		checkAndIfNeedClean(textBox, titlesAndIds);
	});
	
	titlesAndIds.keys().each(function(id) {
			$(id).observe("click", function() {
				setSearchTitle(textBox, titlesAndIds);
			});
	});
	
	setSearchTitle(textBox, titlesAndIds);
}

function setSearchTitle(textBox, titlesAndIds)
{
	if (isTitleText(textBox.value, titlesAndIds))
	{
		textBox.value = getTitleText(titlesAndIds);
		textBox.addClassName("SearchTitle");
	}
}

function checkAndIfNeedClean(textBox, titlesAndIds)
{
	if (isTitleText(textBox.value, titlesAndIds))
	{
		textBox.value = "";
		textBox.removeClassName("SearchTitle");
	}
}

function isTitleText(text, titlesAndIds)
{
	return  text == "" || titlesAndIds.values().indexOf(text) != -1;
}

function getTitleText(titlesAndIds)
{
	var result;
	titlesAndIds.keys().each(function(id) {
			if ($(id).checked)
				result = titlesAndIds.get(id);
		});
	return result;
}

function ValidateSearch(source, args)
{
	if (args.Value == "") {
		args.IsValid = false;
		return
	}
	if (document.getElementById("ctl00_MainContentPlaceHolder_FindRB_3").checked 
		|| document.getElementById("ctl00_MainContentPlaceHolder_FindRB_2").checked)		
		args.IsValid = new RegExp("^\\d{1,10}$$").test(args.Value);
}
