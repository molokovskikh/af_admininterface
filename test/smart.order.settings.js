(function() {
  define(["smart-order-settings", "knockout"], function(m, ko) {
    window.ko = ko;
    module("settings", {
      setup: function() {
        return $("form").validate();
      }
    });
    return test("set validation rules", function() {
      var model, searchModel;
      searchModel = {
        value: ko.observable()
      };
      model = new SettingsViewModel(searchModel);
      model.loader(0);
      model.enableSmartOrder(true);
      searchModel.value("DbfSource");
      equal($("#drugstore_SmartOrderRules_CodeColumn").rules().required, true);
      equal($("#drugstore_SmartOrderRules_QuantityColumn").rules().required, true);
      return equal($("#drugstore_SmartOrderRules_AssortimentPriceCode_Id").rules().required, true);
    });
  });
}).call(this);
