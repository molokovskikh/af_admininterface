window.registerConfirm = ->
	window.confirmed = false
	$("form.confirm").submit (event) ->
		showConfirm = false
		form = $(this)
		validator = form.data("validator")
		if validator
			validator.settings.submitHandler = null

		if not window.confirmed
			confirmInput = form.find(".confirm-empty")

			if confirmInput.length
				value = confirmInput.val()
				unless value or $.trim(value).length
					showConfirm = true

		processConfirm = ->
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

		if showConfirm
			if validator
				validator.settings.submitHandler = processConfirm
			else
				processConfirm()
				return false

unless require?
	$ -> registerConfirm()
