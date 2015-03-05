$ ->
	fillDependedData = window.fillDependedData
	asyncTest "show request dialog", ->
		$("#test_item").attr("checked", "checked")
		url = ""
		isDone = false
		handleDialog = ->
			equal $(".ui-dialog").css("display"), "block"
			$(".ui-dialog input[name=FreePeriodEnd]").val("2012/03/03")
			$(".ui-dialog input[name=AddComment]").val("TestComment")
			$(".ui-dialog button:contains('Продолжить')").click()

		done = (url) ->
			return if isDone
			isDone = true
			equal url, "&FreePeriodEnd=2012%2F03%2F03&AddComment=TestComment"
			start()

		setTimeout handleDialog, 20
		setTimeout done, 30

		fillDependedData url, $("#test_item"), done

	asyncTest "cancel request dialog", ->
		$("#test_item").attr("checked", "checked")
		url = ""
		handleDialog = ->
			equal $(".ui-dialog").css("display"), "block"
			$(".ui-dialog input[name=FreePeriodEnd]").val("21.03.2012")
			$(".ui-dialog button:contains('Отменить')").click()

		done = (url) ->
			return if isDone
			isDone = true
			equal url, null
			start()

		setTimeout handleDialog, 20
		setTimeout done, 30

		fillDependedData url, $("#test_item"), done
