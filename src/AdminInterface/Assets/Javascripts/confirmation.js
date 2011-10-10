(function() {
  $(function() {
    window.confirmed = false;
    return $("form.confirm").submit(function(event) {
      var confirmInput, form, processConfirm, showConfirm, validator, value;
      showConfirm = false;
      form = $(this);
      validator = form.data("validator");
      if (validator) {
        validator.settings.submitHandler = null;
      }
      if (!window.confirmed) {
        confirmInput = form.find(".confirm-empty");
        if (confirmInput.length) {
          value = confirmInput.val();
          if (!(value || $.trim(value).length)) {
            showConfirm = true;
          }
        }
      }
      processConfirm = function() {
        var message;
        message = confirmInput.attr("data-confirm-message");
        return $("<p>" + message + "</p>").dialog({
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
      };
      if (showConfirm) {
        if (validator) {
          return validator.settings.submitHandler = processConfirm;
        } else {
          processConfirm();
          return false;
        }
      }
    });
  });
}).call(this);
