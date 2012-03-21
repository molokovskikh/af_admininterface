(function() {
  var freePeriodEnd, requests;
  requests = Object();
  this.fillDependedData = function(url, element, next) {
    var cancel, request, requestFunction, showRequest;
    request = element.attr("data-request");
    requestFunction = requests[request];
    showRequest = element.attr("checked") && requestFunction;
    if (showRequest) {
      cancel = function() {
        element.removeAttr("checked");
        return element.change();
      };
      return requestFunction(url, next, cancel);
    } else {
      return next(url);
    }
  };
  freePeriodEnd = function(url, next, cancel) {
    var form;
    form = $("<form><div><label>Дата окончания бесплатного периода</label><input name=FreePeriodEnd class='date'></div></form>");
    form.dialog({
      modal: true,
      buttons: {
        "Продолжить": function() {
          if (!$(this).valid()) {
            return;
          }
          url += "&" + $.param($(this).find("input"));
          $(this).dialog("destroy");
          return next(url);
        },
        "Отменить": function() {
          cancel();
          return $(this).dialog("destroy");
        }
      }
    });
    return form.validate();
  };
  requests["FreePeriodEnd"] = freePeriodEnd;
}).call(this);
