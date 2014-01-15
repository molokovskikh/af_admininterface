window.PayerSendMessage = Backbone.View.extend
	el: "body"

	events: {
		"click #users a[data-id]": "selectUser",
		"change #messageReceiverComboBox": "selectUser",
		"change #userMessage_SendToEmail": "showOrHideSubject",
		"change #userMessage_SendToMinimail": "showOrHideSubject"
	}

	initialize: ->
		this.changeUser($("#messageReceiverComboBox").val())
		this.showOrHideSubject()

	showOrHideSubject: () ->
		if $("#userMessage_SendToEmail").prop("checked") or $("#userMessage_SendToMinimail").prop("checked")
			$("#EmailSubjectRow").show()
		else
			$("#EmailSubjectRow").hide()

	selectUser: (event) ->
		val = $(event.target).attr("data-id") ? $(event.target).val()
		this.changeUser(val)

	changeUser: (val) ->
		sendToMinimail = $("#userMessage_SendToMinimail")
		id = parseInt(val)
		if id
			sendToMinimail.prop("disabled", true)
			$("select#messageReceiverComboBox option[selected]").removeAttr("selected")
			$("select#messageReceiverComboBox option[value='#{ id }']").attr("selected", "selected")
		else
			sendToMinimail.prop("disabled", false)
