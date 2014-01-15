setAttribute = (parent, element) ->
	if not parent.prop "checked"
		element.prop "disabled", true
	else
		element.prop "disabled", false

this.trackDependencies = () ->
	$("input[data-depend-on]").each ->
		element = $(this)
		for id in element.attr("data-depend-on").split(" ")
			parent = $("##{id}")
			setAttribute(parent, element)
			parent.change ->
				setAttribute($(this), element)
