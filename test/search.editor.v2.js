(function() {
  define(["search-editor", "knockout"], function(editor, ko) {
    var model;
    window.ko = ko;
    window.searchEditors = {
      test: "test"
    };
    model = null;
    module("search editor", {
      setup: function() {
        var ajaxData, el;
        el = $("<div>");
        el.attr("data-search-title", "test title");
        el.attr("data-search-editor", "test");
        el.append($("<input type=hidden>").data("label", "test label").data("text", "test text").val("1"));
        model = new SearchViewModel(el);
        ajaxData = [
          {
            id: "2",
            name: "test text2"
          }
        ];
        return $.extend({
          ajax: function(data) {
            return data.success(ajaxData);
          }
        });
      }
    });
    test("read init value", function() {
      equal(model.text(), "test text");
      return equal(model.value(), "1");
    });
    test("revert editor value", function() {
      model.edit();
      model.term("123");
      model.search();
      equal(model.result().length, 1);
      model.value("2");
      equal(model.text(), "test text2");
      model.edit();
      equal(model.value(), "");
      return equal(model.text(), "");
    });
    return test("in search mode value undefined", function() {
      model.edit();
      equal(model.value(), "");
      model.cancel();
      equal(model.value(), "1");
      return equal(model.text(), "test text");
    });
  });
}).call(this);
