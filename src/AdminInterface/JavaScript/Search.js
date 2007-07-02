var TitlesAndIds = $H({"FindRB_0" : "Поиска по имени", 
				   "FindRB_1" : "Поиска по коду",
				   "FindRB_2" : "Поиска по биллинг коду",
				   "FindRB_3" : "Поиска по логину",
				   "FindRB_4" : "Поиска по юридическому наименованию"});

function SetSearchTitle()
{
	textBox = document.getElementById("FindTB");
	if (IsTitleText(textBox.value))
	{
		textBox.value = GetTitleText();
		textBox.className = "SearchTitle";
	}
}

function IsTitleText(text)
{
	return  text == "" || TitlesAndIds.values().indexOf(text) != -1;
}

function GetTitleText()
{
	var result;
	TitlesAndIds.keys().each(function(id)
		{
			if ($(id).checked)
				result = TitlesAndIds[id];
		});
	return result;
}

function CheckAndIfNeedClean(textBox)
{
	if (IsTitleText(textBox.value))
	{
		textBox.value = "";
		textBox.className = "";
	}
}
