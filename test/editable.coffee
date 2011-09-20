$ ->
	test "add row into editable table", ->
		$("#with_thead").data "template", -> $("<tr></tr>")
		$("#with_thead a.add").click()
		equal $("#with_thead tbody tr").length, 2
