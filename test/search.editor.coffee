$ ->
	ajaxData = [{id: 1, name: "1"}, {id: 2, name: "2"}]
	$.extend
		ajax: (data) ->
			data.success(ajaxData)

	test "search without item", ->
		$("input[type=button].search").data("url", "test")

		$(".search").click()
		equal $("select option").length, 2

	test "hide settings on search activation", ->
		activateSearch()
		equal $("#activate_block .settings").css("display"), "none"

	test "cancel search", ->
		activateSearch()

		context = $("#activate_block")
		context.find("#do").attr("checked", true)
		context.find("#do").click()

		context.find(".cancel-search").click()
		equal context.find("#on").attr("checked"), null
		equal context.find(".search").length, 0
