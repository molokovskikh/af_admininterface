var TitlesAndIds = $H({	"FindRB_0" : "Автоматический выбор типа поиска", 
						"FindRB_1" : "Поиска по имени", 
						"FindRB_2" : "Поиска по коду",
						"FindRB_3" : "Поиска по биллинг коду",
						"FindRB_4" : "Поиска по логину",
						"FindRB_5" : "Поиска по юридическому наименованию"});

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

function ValidateSearch(source, args)
{
	if (IsTitleText(args.Value))
		args.IsValid = false
	if (document.getElementById("FindRB_3").checked 
		|| document.getElementById("FindRB_2").checked)		
		args.IsValid = new RegExp("^\\d{1,10}$$").test(args.Value);
}
