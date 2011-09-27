$ ->
	submited = false
	submitResult = false
	onSubmit = null
	#порядок регистрации важен! последним должен регистрировать тестовый обработчик
	$("#confirm_validation").validate()
	$("form").submit (event) ->
		result = event.result
		submited = true
		if event.result == false
			submitResult = false
		else
			submitResult = true
		if onSubmit
			callback = ->
				onSubmit(result)
			setTimeout callback, 10
		return false

	module "confirmation",
		setup: ->
			$(".ui-dialog").dialog("destroy")
			submited = false
			submitResult = false
			onSubmit = null
			window.confirmed = false
		teardown: ->
			$(".ui-dialog").dialog("destroy")
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

	test "do not show confirm if field filled", ->
		$("#update input").val("test")

		onSubmit = ->
			equal $(".ui-dialog").length, 0
			equal submitResult, true
			start()

		$("#update").submit()
		stop()

	test "do not show confirm if form invalid", ->
		onSubmit = afterSubmit = ->
			equal $(".ui-dialog").length, 0
			start()

		$("#confirm_validation").submit()
		stop()

	test "confirm valid form", ->
		$("#confirm_validation .required").val("test")

		onSubmit = afterSubmit = ->
			onSubmit = afterContinue
			$(".ui-dialog button:contains('Продолжить')").click()

		submitCount = 0
		afterContinue = (result) ->
			submitCount++
			if submitCount == 1
				equal result, true, "не отправили форму"
			else
				equal result, false, "ложное подтверждение от jquery validator"
				start()

		$("#confirm_validation").submit()
		stop()
