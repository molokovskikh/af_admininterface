define ["payer-search"], ->
	$ ->
		module "filter",
			setup: ->

		$.extend
			get: (url, data, callback) ->
				run = () ->  callback([])
				setTimeout run, 100

		test "hide not found message", ->
			$("#PayerExists").attr("checked", "checked")
			$("#PayerExists").click()

			$("#SearchPayerTextPattern").val("тест")
			$("#SearchPayerButton").click()
			afterClick = ->
				equal $("#MessageForPayer").text(), "Ничего не найдено", "ничего не найдено"
				$("#PayerExists").removeAttr("checked")
				$("#PayerExists").click()

				equal $("#MessageForPayer").text(), "", "сообщение должно быть пустым"
				start()

			setTimeout afterClick, 100
			stop()
