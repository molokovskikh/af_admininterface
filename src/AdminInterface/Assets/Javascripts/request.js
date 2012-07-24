(function() {
  var addComment, freePeriodEnd, requests, showForm;
  requests = Object();
  this.fillDependedData = function(url, element, next) {
    var cancel, request, requestFunction, showRequest;
    request = element.attr("data-request");
    requestFunction = requests[request];
    showRequest = (element.attr("checked") && requestFunction && !element.attr("unchecked")) || (!element.attr("checked") && requestFunction && element.attr("unchecked"));
    if (showRequest) {
      cancel = function() {
        if (!element.attr("unchecked")) {
          element.removeAttr("checked");
        } else {
          element.attr("checked", true);
        }
        return element.change();
      };
      return requestFunction(url, next, cancel);
    } else {
      return next(url);
    }
  };
  showForm = function(url, next, cancel, form) {
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
  freePeriodEnd = function(url, next, cancel) {
    var form;
    form = $("<form><div><label>Дата окончания бесплатного периода</label><input name=FreePeriodEnd class='date'></div></form>");
    return showForm(url, next, cancel, form);
  };
  addComment = function(url, next, cancel) {
    var form;
    form = $("<form><div><label>Введите причину отключения</label><input name=AddComment class='required' ></div></form>");
    return showForm(url, next, cancel, form);
  };
  requests["FreePeriodEnd"] = freePeriodEnd;
  requests["AddComment"] = addComment;
}).call(this);
