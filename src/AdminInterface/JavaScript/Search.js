var TitlesAndIds = $H({	"ctl00_MainContentPlaceHolder_FindRB_0" : "�������������� ����� ���� ������", 
						"ctl00_MainContentPlaceHolder_FindRB_1" : "������ �� �����", 
						"ctl00_MainContentPlaceHolder_FindRB_2" : "������ �� ����",
						"ctl00_MainContentPlaceHolder_FindRB_3" : "������ �� ������� ����",
						"ctl00_MainContentPlaceHolder_FindRB_4" : "������ �� ������",
						"ctl00_MainContentPlaceHolder_FindRB_5" : "������ �� ������������ ������������"});

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
				result = TitlesAndIds.get(id);
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
	if (document.getElementById("ctl00_MainContentPlaceHolder_FindRB_3").checked 
		|| document.getElementById("ctl00_MainContentPlaceHolder_FindRB_2").checked)		
		args.IsValid = new RegExp("^\\d{1,10}$$").test(args.Value);
}
