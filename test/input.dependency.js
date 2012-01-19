(function() {
  var trackDependencies;
  trackDependencies = this.trackDependencies;
  $(function() {
    return test("disable input if dependency off", function() {
      trackDependencies();
      equal($("input[type=textbox]").attr("disabled"), "disabled");
      $("#on-box").click();
      equal($("input[type=textbox]").attr("disabled"), null);
      $("#on-box").click();
      return equal($("input[type=textbox]").attr("disabled"), "disabled");
    });
  });
}).call(this);
