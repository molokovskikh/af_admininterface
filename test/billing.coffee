$ ->
	test "show save message", ->
		$.extend
			ajax: (data) ->
				data.success()
		success = ->
		fail = ->
		AjaxRequest "test", success, fail
		equal $("#ErrorMessageDiv").html(), "Сохранено"
