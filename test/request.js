(function() {
  $(function() {
    var fillDependedData;
    fillDependedData = window.fillDependedData;
    asyncTest("show request dialog", function() {
      var done, handleDialog, isDone, url;
      $("#test_item").attr("checked", "checked");
      url = "";
      isDone = false;
      handleDialog = function() {
        equal($(".ui-dialog").css("display"), "block");
        $(".ui-dialog input[name=FreePeriodEnd]").val("03.03.2012");
        return $(".ui-dialog button:contains('Продолжить')").click();
      };
      done = function(url) {
        if (isDone) {
          return;
        }
        isDone = true;
        equal(url, "&FreePeriodEnd=03.03.2012");
        return start();
      };
      setTimeout(handleDialog, 20);
      setTimeout(done, 30);
      return fillDependedData(url, $("#test_item"), done);
    });
    return asyncTest("cancel request dialog", function() {
      var done, handleDialog, url;
      $("#test_item").attr("checked", "checked");
      url = "";
      handleDialog = function() {
        equal($(".ui-dialog").css("display"), "block");
        $(".ui-dialog input[name=FreePeriodEnd").val("21.03.2012");
        return $(".ui-dialog button:contains('Отменить')").click();
      };
      done = function(url) {
        var isDone;
        if (isDone) {
          return;
        }
        isDone = true;
        equal(url, null);
        return start();
      };
      setTimeout(handleDialog, 20);
      setTimeout(done, 30);
      return fillDependedData(url, $("#test_item"), done);
    });
  });
}).call(this);
