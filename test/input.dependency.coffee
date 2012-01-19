trackDependencies = this.trackDependencies
$ ->
	test "disable input if dependency off", ->
		trackDependencies()
		equal $("input[type=textbox]").attr("disabled"), "disabled"
		$("#on-box").click() #checked
		equal $("input[type=textbox]").attr("disabled"), null
		$("#on-box").click() #un checked
		equal $("input[type=textbox]").attr("disabled"), "disabled"

