(function() {
  var addComment, freePeriodEnd, requests, showForm;

  requests = Object();

  this.fillDependedData = function(url, element, next) {
    var cancel, request, requestFunction, showRequest;
    request = element.attr("data-request");
    requestFunction = requests[request];
    showRequest = (element.prop("checked") && requestFunction && !element.attr("unchecked")) || (!element.prop("checked") && requestFunction && element.attr("unchecked"));
    if (showRequest) {
      cancel = function() {
        if (!element.attr("unchecked")) {
          element.prop("checked", false);
        } else {
          element.prop("checked", true);
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
          var params;
          if (!$(this).valid()) {
            return;
          }
          params = url;
          url = $(this).children("div").children("input").each(function() {
            return params += "&" + $.param($(this));
          });
          $(this).dialog("destroy");
          return next(params);
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
    form = $("<form><div><label>Дата окончания бесплатного периода</label><input name=FreePeriodEnd class='date'> </br>" + "<label>Основание бесплатного обслуживания</label><input id='AddCommentField' name='AddComment' class='required' >" + "</div></form>");
    return showForm(url, next, cancel, form);
  };

  addComment = function(url, next, cancel) {
    var form;
    form = $("<form onsubmit='return false;'><div><label>Введите причину отключения</label><input id='AddCommentField' name='AddComment' class='required' ></div></form>");
    return showForm(url, next, cancel, form);
  };

  requests["FreePeriodEnd"] = freePeriodEnd;

  requests["AddComment"] = addComment;

}).call(this);
