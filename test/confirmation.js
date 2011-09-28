(function() {
  $(function() {
    var onSubmit, submitResult, submited;
    submited = false;
    submitResult = false;
    onSubmit = null;
    $("#confirm_validation").validate();
    $("form").submit(function(event) {
      var callback, result;
      result = event.result;
      submited = true;
      if (event.result === false) {
        submitResult = false;
      } else {
        submitResult = true;
      }
      if (onSubmit) {
        callback = function() {
          return onSubmit(result);
        };
        setTimeout(callback, 10);
      }
      return false;
    });
    module("confirmation", {
      setup: function() {
        $(".ui-dialog").dialog("destroy");
        submited = false;
        submitResult = false;
        onSubmit = null;
        return window.confirmed = false;
      },
      teardown: function() {
        $(".ui-dialog").dialog("destroy");
        onSubmit = null;
        return window.confirmed = false;
      }
    });
    test("request confirmation", function() {
      var afterContinue, afterSubmit;
      onSubmit = afterSubmit = function() {
        equal(submitResult, false);
        equal($(".ui-dialog").css("display"), "block");
        onSubmit = afterContinue;
        return $(".ui-dialog button:contains('Продолжить')").click();
      };
      afterContinue = function() {
        equal(submitResult, true);
        return start();
      };
      $("#update").submit();
      return stop();
    });
    test("do not submit form on cancel", function() {
      var afterCancel, afterSubmit;
      onSubmit = afterSubmit = function() {
        onSubmit = null;
        submited = false;
        $(".ui-dialog button:contains('Отменить')").click();
        return setTimeout(afterCancel, 20);
      };
      afterCancel = function() {
        equal(submited, false);
        return start();
      };
      $("#update").submit();
      return stop();
    });
    test("do not show confirm if field filled", function() {
      $("#update input").val("test");
      onSubmit = function() {
        equal($(".ui-dialog").length, 0);
        equal(submitResult, true);
        return start();
      };
      $("#update").submit();
      return stop();
    });
    test("do not show confirm if form invalid", function() {
      var afterSubmit;
      onSubmit = afterSubmit = function() {
        equal($(".ui-dialog").length, 0);
        return start();
      };
      $("#confirm_validation").submit();
      return stop();
    });
    test("confirm valid form", function() {
      var afterContinue, afterSubmit;
      $("#confirm_validation .required").val("test");
      onSubmit = afterSubmit = function() {
        onSubmit = afterContinue;
        return $(".ui-dialog button:contains('Продолжить')").click();
      };
      afterContinue = function(result) {
        equal(result, true, "не отправили форму");
        return start();
      };
      $("#confirm_validation").submit();
      return stop();
    });
    return asyncTest("do not confirm if input filled", function() {
      $("#confirm_validation .required").val("test");
      $("#confirm_validation .confirm-empty").val("test");
      onSubmit = function(result) {
        equal(result, true);
        equal($(".ui-dialog").length, 0);
        return start();
      };
      return $("#confirm_validation").submit();
    });
  });
}).call(this);
