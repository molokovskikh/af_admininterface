(function() {
  $(function() {
    test("set is hidden for supplier if register copy for supplier", function() {
      var model;
      model = new RegistrationViewModel();
      model.forSupplier(true);
      equal(model.isHiddenFromSupplier(), true);
      return equal(model.isHiddenFromSupplierEnable(), false);
    });
    return test("update orgs after payer id change", function() {
      var ajaxData, model;
      model = new RegistrationViewModel();
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
        get: function(url, data, callback) {
          return callback(ajaxData);
        }
      });
      model.payerId(1);
      return equal(model.orgs().length, 2);
    });
  });
}).call(this);
