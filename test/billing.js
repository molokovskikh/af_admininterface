(function() {
  $(function() {
    return test("show save message", function() {
      var fail, success;
      $.extend({
        ajax: function(data) {
          return data.success();
        }
      });
      success = function() {};
      fail = function() {};
      AjaxRequest("test", success, fail);
      return equal($("#ErrorMessageDiv").html(), "Сохранено");
    });
  });
}).call(this);
