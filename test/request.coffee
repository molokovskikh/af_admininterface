$ ->
	fillDependedData = window.fillDependedData
	asyncTest "show request dialog", ->
		$("#test_item").attr("checked", "checked")
		url = ""
		isDone = false
		handleDialog = ->
			equal $(".ui-dialog").css("display"), "block"
			$(".ui-dialog input[name=FreePeriodEnd]").val("03.03.2012")
			$(".ui-dialog button:contains('Продолжить')").click()

		done = (url) ->
			return if isDone
			isDone = true
			equal url, "&FreePeriodEnd=03.03.2012"
			start()

		setTimeout handleDialog, 20
		setTimeout done, 30

		fillDependedData url, $("#test_item"), done

	asyncTest "cancel request dialog", ->
		$("#test_item").attr("checked", "checked")
		url = ""
		handleDialog = ->
			equal $(".ui-dialog").css("display"), "block"
			$(".ui-dialog input[name=FreePeriodEnd").val("21.03.2012")
			$(".ui-dialog button:contains('Отменить')").click()

		done = (url) ->
			return if isDone
			isDone = true
			equal url, null
			start()

		setTimeout handleDialog, 20
		setTimeout done, 30

		fillDependedData url, $("#test_item"), done
