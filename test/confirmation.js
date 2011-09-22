(function() {
  $(function() {
    var onSubmit, submitResult, submited;
    window.confirmed = false;
    $("form.confirm").submit(function(event) {
      var form, processConfirm, validator;
      if (window.confirmed) {
        return;
      }
      form = $(this);
      validator = form.data("validator");
      processConfirm = function() {
        var confirmInput, message;
        confirmInput = form.find(".confirm-empty");
        if (confirmInput.length) {
          message = confirmInput.attr("data-confirm-message");
          $("<p>" + message + "</p>").dialog({
            modal: true,
            buttons: {
              "Продолжить": function() {
                window.confirmed = true;
                $(this).dialog("destroy");
                return form.submit();
              },
              "Отменить": function() {
                return $(this).dialog("destroy");
              }
            }
          });
          return false;
        }
      };
      if (validator) {
        return validator.settings.submitHandler = processConfirm;
      } else {
        return processConfirm();
      }
    });
    submited = false;
    submitResult = false;
    onSubmit = null;
    $("form").submit(function(event) {
      submited = true;
      if (event.result === false) {
        submitResult = false;
      } else {
        submitResult = true;
      }
      if (onSubmit) {
        setTimeout(onSubmit, 30);
      }
      return false;
    });
    module("test", {
      setup: function() {
        submited = false;
        submitResult = false;
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
    return test("show confirm if form valid", function() {
      var afterSubmit;
      $("#confirm_validation").validate();
      onSubmit = afterSubmit = function() {
        equal($(".ui-dialog").length, 0);
        return start();
      };
      $("#confirm_validation").submit();
      return stop();
    });
  });
}).call(this);
