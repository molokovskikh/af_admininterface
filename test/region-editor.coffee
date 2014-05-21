define ["region-editor"], ->
	$ ->
		module "confirmation",
			setup: ->
				window.regionCount = 1

		test "request confirmation", ->
			ShowRegions()
			regionId = $("#RegionsTableBody input[type=hidden]").val()
			equal regionId, "144115188075855872"
