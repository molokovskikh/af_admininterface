setAttribute = (parent, element) ->
	if not parent.attr "checked"
		element.attr "disabled", "disabled"
	else
		element.removeAttr "disabled"

this.trackDependencies = () ->
	$("input[data-depend-on]").each ->
		element = $(this)
		for id in element.attr("data-depend-on").split(" ")
			parent = $("##{id}")
			setAttribute(parent, element)
			parent.change ->
				setAttribute($(this), element)
