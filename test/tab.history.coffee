$ ->
	test "support tab history", ->
		equal $("#test1-tab").css("display"), "block"
		equal $("#test2-tab").css("display"), "none"
		$("#test2").click()
		afterClick = ->
			equal $("#test1-tab").css("display"), "none"
			equal $("#test2-tab").css("display"), "block"
			history.back()
			setTimeout afterBack, 50

		afterBack = -> 
			equal $("#test1-tab").css("display"), "block"
			equal $("#test2-tab").css("display"), "none"
			start()
		setTimeout afterClick, 50
		stop()

