(function() {
  $(function() {
    return test("support tab history", function() {
      var afterBack, afterClick;
      equal($("#test1-tab").css("display"), "block");
      equal($("#test2-tab").css("display"), "none");
      $("#test2").click();
      afterClick = function() {
        equal($("#test1-tab").css("display"), "none");
        equal($("#test2-tab").css("display"), "block");
        history.back();
        return setTimeout(afterBack, 50);
      };
      afterBack = function() {
        equal($("#test1-tab").css("display"), "block");
        equal($("#test2-tab").css("display"), "none");
        return start();
      };
      setTimeout(afterClick, 50);
      return stop();
    });
  });
}).call(this);
