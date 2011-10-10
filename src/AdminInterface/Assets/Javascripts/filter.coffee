$ ->
	allLabel = "Показать для всех"
	currentLabel = "Показать только для текущего"

	$(".filter a").click ->
		currentFilter = $(".current-filter")
		if currentFilter.length
			unfilter()
			currentFilter.removeClass("current-filter")

		$(this).addClass "current-filter"
		$(".filter-checker").text(allLabel).show()
		filter()

	$(".filter-checker").click -> 
		if $(this).text() == allLabel
			unfilter()
			$(this).text(currentLabel)
		else
			filter()
			$(this).text(allLabel)

	filter = () ->
		filterValue = $(".current-filter").attr("data-filter")
		$(".filtrable tbody tr[data-filter!=#{ filterValue }]").hide()

	unfilter = () ->
		$(".filtrable tr").show()