$ ->
	window.confirmed = false

	$("form.confirm").submit (event) ->
		if window.confirmed
			return

		form = $(this)
		validator = form.data("validator")

		processConfirm = ->
			confirmInput = form.find(".confirm-empty")
			if confirmInput.length
				message = confirmInput.attr("data-confirm-message")
				$("<p>#{message}</p>").dialog
					modal: true,
					buttons:
						"Продолжить": ->
							window.confirmed = true
							$(this).dialog("destroy")
							form.submit()
						"Отменить": ->
							$(this).dialog("destroy")
				return false

		if validator
			validator.settings.submitHandler = processConfirm
		else
			processConfirm()

	submited = false
	submitResult = false
	onSubmit = null
	$("form").submit (event) -> 
		submited = true
		if event.result == false
			submitResult = false
		else
			submitResult = true
		if onSubmit
			setTimeout onSubmit, 30
		return false

	module "test",
		setup: ->
			submited = false
			submitResult = false
			onSubmit = null
			window.confirmed = false

	test "request confirmation", ->
		onSubmit = afterSubmit = ->
			equal submitResult, false
			equal $(".ui-dialog").css("display"), "block"
			onSubmit = afterContinue
			$(".ui-dialog button:contains('Продолжить')").click()

		afterContinue = -> 
			equal submitResult, true
			start()

		$("#update").submit()
		stop()

	test "do not submit form on cancel", -> 
		onSubmit = afterSubmit = ->
			onSubmit = null
			submited = false
			$(".ui-dialog button:contains('Отменить')").click()
			setTimeout afterCancel, 20

		afterCancel = -> 
			equal submited, false
			start()

		$("#update").submit()
		stop()

	test "show confirm if form valid", ->
		$("#confirm_validation").validate()

		onSubmit = afterSubmit = ->
			equal $(".ui-dialog").length, 0
			start()

		$("#confirm_validation").submit()
		stop()