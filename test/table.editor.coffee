$ ->
	test "create new row in empty table", ->
		$("#empty a.new").click()
		equal $("#empty tbody tr").length, 1

	test "create new row in not empty table", ->
		$("#row1 a.new").click()
		equal $("#row1 tbody tr").length, 2
