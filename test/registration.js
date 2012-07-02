(function() {
  $(function() {
    return test("set is hidden for supplier if register copy for supplier", function() {
      var model;
      model = new RegistrationViewModel();
      model.forSupplier(true);
      equal(model.isHiddenFromSupplier(), true);
      return equal(model.isHiddenFromSupplierEnable(), false);
    });
  });
}).call(this);
