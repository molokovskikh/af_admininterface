(function() {
  window.PayerSendMessage = Backbone.View.extend({
    el: "body",
    events: {
      "click #users a[data-id]": "selectUser",
      "change #messageReceiverComboBox": "selectUser",
      "change #userMessage_SendToEmail": "showOrHideSubject",
      "change #userMessage_SendToMinimail": "showOrHideSubject"
    },
    initialize: function() {
      this.changeUser($("#messageReceiverComboBox").val());
      return this.showOrHideSubject();
    },
    showOrHideSubject: function() {
      if ($("#userMessage_SendToEmail").prop("checked") || $("#userMessage_SendToMinimail").prop("checked")) {
        return $("#EmailSubjectRow").show();
      } else {
        return $("#EmailSubjectRow").hide();
      }
    },
    selectUser: function(event) {
      var val, _ref;
      val = (_ref = $(event.target).attr("data-id")) != null ? _ref : $(event.target).val();
      return this.changeUser(val);
    },
    changeUser: function(val) {
      var id, sendToMinimail;
      sendToMinimail = $("#userMessage_SendToMinimail");
      id = parseInt(val);
      if (id) {
        sendToMinimail.prop("disabled", true);
        $("select#messageReceiverComboBox option[selected]").removeAttr("selected");
        return $("select#messageReceiverComboBox option[value='" + id + "']").attr("selected", "selected");
      } else {
        return sendToMinimail.prop("disabled", false);
      }
    }
  });

}).call(this);
