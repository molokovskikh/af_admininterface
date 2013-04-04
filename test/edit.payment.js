(function() {
  $(function() {
    return test("", function() {
      var ajaxData, el, href;
      window.siteroot = "/AdminInterface";
      ajaxData = [
        {
          id: 1,
          label: "1"
        }, {
          id: 2,
          label: "2"
        }
      ];
      $.extend({
        ajax: function(data) {
          return data.success(ajaxData);
        }
      });
      el = $("#payment_Payer_Name");
      el.val("test");
      el.autocomplete("search");
      href = $(".ui-menu-item a").attr("href");
      return equal(href, "/AdminInterface/Billing/Edit?billingcode=1");
    });
  });
}).call(this);
