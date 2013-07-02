define ["search-editor", "knockout"], (editor, ko) ->
	window.ko = ko
	window.searchEditors = { test: "test" }
	model = null
	module "search editor",
		setup: ->
			el = $("<div>")
			el.attr("data-search-title", "test title")
			el.attr("data-search-editor", "test")
			el.append($("<input type=hidden>").data("label", "test label").data("text", "test text").val("1"))
			model = new SearchViewModel(el)
			ajaxData = [{id: "2", name: "test text2"}]
			$.extend
				ajax: (data) ->
					data.success(ajaxData);

	test "read init value", ->
		equal model.text(), "test text"
		equal model.value(), "1"

	test "revert editor value", ->
		model.edit()
		model.term("123")
		model.search()

		equal model.result().length, 1
		model.value("2")
		equal model.text(), "test text2"
		model.edit()
		equal model.value(), ""
		equal model.text(), ""

	test "in search mode value undefined", ->
		model.edit()
		equal model.value(), ""
		model.cancel()
		equal model.value(), "1"
		equal model.text(), "test text"
