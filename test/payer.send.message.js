(function() {
  $(function() {
    test("select user", function() {
      var flag, view;
      view = new PayerSendMessage();
      flag = $("#message_SendToMinimail");
      equal(flag.attr("disabled"), "disabled");
      $("#messageReceiverComboBox option:last").attr("selected", "selected");
      $("#messageReceiverComboBox").change();
      return equal(flag.attr("disabled"), null);
    });
    return test("select user", function() {
      var view;
      view = new PayerSendMessage();
      $("a[data-id=1]").click();
      return equal($("#messageReceiverComboBox option:selected").val(), "1");
    });
  });
}).call(this);
