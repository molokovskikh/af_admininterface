var TitlesAndIds = $H({	"FindRB_0" : "�������������� ����� ���� ������", 
						"FindRB_1" : "������ �� �����", 
						"FindRB_2" : "������ �� ����",
						"FindRB_3" : "������ �� ������� ����",
						"FindRB_4" : "������ �� ������",
						"FindRB_5" : "������ �� ������������ ������������"});

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
