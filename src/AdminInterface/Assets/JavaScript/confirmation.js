(function() {
  $(function() {
    window.confirmed = false;
    return $("form.confirm").submit(function(event) {
      var form, processConfirm, validator;
      form = $(this);
      validator = form.data("validator");
      if (window.confirmed) {
        return;
      }
      processConfirm = function() {
        var confirmInput, message, value;
        if (window.confirmed) {
          validator.settings.submitHandler = null;
          form.submit();
          return;
        }
        confirmInput = form.find(".confirm-empty");
        if (confirmInput.length) {
          value = confirmInput.val();
          if (value && $.trim(value).length) {
            return;
          }
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
        validator.settings.submitHandler = processConfirm;
        return true;
      } else {
        return processConfirm();
      }
    });
  });
}).call(this);
