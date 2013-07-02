define ["confirmation"], ->
	$ ->
		submited = false
		submitResult = false
		onSubmit = null

		module "confirmation",
			setup: ->
				$(".ui-dialog").dialog("destroy")
				submited = false
				submitResult = false
				onSubmit = null
				window.confirmed = false
				#порядок регистрации важен! последним должен регистрировать тестовый обработчик
				$("form").submit (event) ->
					event.preventDefault()
					#return false
				registerConfirm()
				$("#confirm_validation").validate()
				$("form").submit (event) ->
					result = event.result
					if event.result == false
						submitResult = false
					else
						submitResult = true
					if onSubmit
						callback = ->
							onSubmit(result)
						setTimeout callback, 10
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

			afterContinue = (result) ->
				equal result, true, "не отправили форму"
				start()

			$("#confirm_validation").submit()
			stop()

		test "do not confirm if input filled", ->
			$("#confirm_validation .required").val("test")
			$("#confirm_validation .confirm-empty").val("test")

			onSubmit = (result) ->
				equal result, true
				equal $(".ui-dialog").length, 0
				start()

			$("#confirm_validation").submit()
			stop()
