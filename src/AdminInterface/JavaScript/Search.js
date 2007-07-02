var TitlesAndIds = $H({"FindRB_0" : "������ �� �����", 
				   "FindRB_1" : "������ �� ����",
				   "FindRB_2" : "������ �� ������� ����",
				   "FindRB_3" : "������ �� ������",
				   "FindRB_4" : "������ �� ������������ ������������"});

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
