$ ->
	test "create new row in empty table", ->
		$("#empty a.new").click()
		equal $("#empty tbody tr").length, 1

	test "create new row in not empty table", ->
		$("#row1 a.new").click()
		equal $("#row1 tbody tr").length, 2

	test "increment index in name", ->
		$("#row1 a.new").click()
		equal $("#row1 input").get(0).name, "items[0].Name"
		equal $("#row1 input").get(1).name, "items[1].Name"
