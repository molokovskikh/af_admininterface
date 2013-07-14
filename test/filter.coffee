define ["filter"], ->
	$ ->
		module "filter",
			setup: ->
				window.registerFilter()

		test "filter table rows", ->
			link = $(".filter a[data-filter=f1]")
			equal link.length, 1
			link.click()
			equal $("tr[data-filter=f2]").css("display"), "none"
			notEqual $("tr[data-filter=f1]").css("display"), "none"

		test "filter checker", ->
			$(".filter-checker").hide()
			checker = $(".filter-checker")
			equal checker.css("display"), "none"
			link = $(".filter a[data-filter=f1]")
			link.click()
			notEqual checker.css("display"), "none"
			checker.click()
			notEqual $("tr[data-filter=f1]").css("display"), "none"
			notEqual $("tr[data-filter=f2]").css("display"), "none"
			checker.click()
			equal $("tr[data-filter=f2]").css("display"), "none"
			notEqual $("tr[data-filter=f1]").css("display"), "none"

		test "filter items", ->
			$(".filter a[data-filter=f1]").click()
			equal $("tr[data-filter=f2]").css("display"), "none"
			notEqual $("tr[data-filter=f1]").css("display"), "none"

			$(".filter a[data-filter=f2]").click()
			notEqual $("tr[data-filter=f2]").css("display"), "none"
			equal $("tr[data-filter=f1]").css("display"), "none"
