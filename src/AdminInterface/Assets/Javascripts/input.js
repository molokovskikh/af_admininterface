(function() {
  var setAttribute;

  setAttribute = function(parent, element) {
    if (!parent.attr("checked")) {
      return element.attr("disabled", "disabled");
    } else {
      return element.removeAttr("disabled");
    }
  };

  this.trackDependencies = function() {
    return $("input[data-depend-on]").each(function() {
      var element, id, parent, _i, _len, _ref, _results;
      element = $(this);
      _ref = element.attr("data-depend-on").split(" ");
      _results = [];
      for (_i = 0, _len = _ref.length; _i < _len; _i++) {
        id = _ref[_i];
        parent = $("#" + id);
        setAttribute(parent, element);
        _results.push(parent.change(function() {
          return setAttribute($(this), element);
        }));
      }
      return _results;
    });
  };

}).call(this);
