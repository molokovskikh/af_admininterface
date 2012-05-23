$ ->
	$("form.autosubmit").find("input, select").change ->
		$(this).parents("form").submit()
