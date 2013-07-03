(function() {
  define(["smart-order-settings", "knockout"], function(m, ko) {
    window.ko = ko;
    module("settings", {
      setup: function() {
        return $("form").validate();
      }
    });
    test("set validation rules", function() {
      var model, searchModel;
      searchModel = {
        value: ko.observable("DbfSource")
      };
      model = new SettingsViewModel(searchModel);
      model.loader(0);
      model.enableSmartOrder(true);
      searchModel.value("DbfSource");
      equal($("#drugstore_SmartOrderRules_CodeColumn").rules().required, true);
      equal($("#drugstore_SmartOrderRules_QuantityColumn").rules().required, true);
      return equal($("#drugstore_SmartOrderRules_AssortimentPriceCode_Id").rules().required, true);
    });
    test("disable validation if source not cofigurable", function() {
      var model, searchModel;
      searchModel = {
        value: ko.observable("TestSource")
      };
      model = new SettingsViewModel(searchModel);
      model.loader(0);
      model.enableSmartOrder(true);
      return equal($("#drugstore_SmartOrderRules_CodeColumn").rules().required, void 0);
    });
    return test("update validation on parser change", function() {
      var model, searchModel;
      searchModel = {
        value: ko.observable("TestSource")
      };
      model = new SettingsViewModel(searchModel);
      model.loader(0);
      model.enableSmartOrder(true);
      searchModel.value("DbfSource");
      equal($("#drugstore_SmartOrderRules_CodeColumn").rules().required, true);
      searchModel.value("TestSource");
      return equal($("#drugstore_SmartOrderRules_CodeColumn").rules().required, void 0);
    });
  });
}).call(this);
