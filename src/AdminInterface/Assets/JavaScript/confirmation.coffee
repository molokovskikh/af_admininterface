$ ->
	window.confirmed = false
	$("form.confirm").submit (event) ->
		form = $(this)
		validator = form.data("validator")

		return if window.confirmed

		processConfirm = ->

			if window.confirmed
				validator.settings.submitHandler = null
				form.submit()
				return

			confirmInput = form.find(".confirm-empty")
			if confirmInput.length
				value = confirmInput.val()
				if value and $.trim(value).length
					return
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
			return true
		else
			processConfirm()
