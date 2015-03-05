$ ->
	test "disable on select user", ->
		view = new PayerSendMessage()
		flag = $("#userMessage_SendToMinimail")
		equal flag.prop("disabled"), true
		$("#messageReceiverComboBox option:last").prop("selected", true)
		$("#messageReceiverComboBox").change()
		equal flag.prop("disabled"), false

	test "select user", ->
		view = new PayerSendMessage()
		$("a[data-id=1]").click()
		equal $("#messageReceiverComboBox option:selected").val(), "1"
