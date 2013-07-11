$ ->
	test "fire add event", ->
		regions = new Regions([name: "test"])
		added = null
		regions.bind "add", (region) ->
			return added = region
		regions.add(name: "test1")
		equal added.get("name"), "test1"

	test "add regional delivery group on region add", ->
		$("#groups").children().remove()
		window.regions = new Regions([name: "test"])
		view = new GroupListView(regions: regions)

		regions.add(name: "test1")
		element = $("#groups").children().get(0)
		equal $(element).html(), "test1"

	test "remove group on region delete", ->
		$("#groups").children().remove()
		window.regions = new Regions([name: "test"])
		view = new GroupListView(regions: regions)

		regions.add(name: "test1")
		equal $("#groups").children().length, 1
		regions.remove(regions.at(1))
		equal $("#groups").children().length, 0
