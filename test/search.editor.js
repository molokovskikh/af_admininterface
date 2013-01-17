(function() {
  $(function() {
    test("search without item", function() {
      var ajaxData;
      $("input[type=button].search").data("url", "test");
      ajaxData = [
        {
          id: 1,
          name: "1"
        }, {
          id: 2,
          name: "2"
        }
      ];
      $.extend({
        ajax: function(data) {
          return data.success(ajaxData);
        }
      });
      $(".search").click();
      return equal($("select option").length, 2);
    });
    test("hide settings on search activation", function() {
      activateSearch();
      return equal($("#activate_block .settings").css("display"), "none");
    });
    return test("cancel search", function() {
      var context;
      activateSearch();
      context = $("#activate_block");
      context.find("#do").attr("checked", true);
      context.find("#do").click();
      context.find(".cancel-search").click();
      equal(context.find("#on").attr("checked"), null);
      return equal(context.find(".search").length, 0);
    });
  });
}).call(this);
