$ ->
	test "select user", ->
		view = new PayerSendMessage()
		flag = $("#message_SendToMinimail")
		equal flag.attr("disabled"), "disabled"
		$("#messageReceiverComboBox option:last").attr("selected", "selected")
		$("#messageReceiverComboBox").change()
		equal flag.attr("disabled"), null

	test "select user", ->
		view = new PayerSendMessage()
		$("a[data-id=1]").click()
		equal $("#messageReceiverComboBox option:selected").val(), "1"
