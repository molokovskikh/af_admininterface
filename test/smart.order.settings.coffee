define ["smart-order-settings", "knockout"], (m, ko) ->
	window.ko = ko
	module "settings",
		setup: ->
			$("form").validate()
	test "set validation rules", ->
		searchModel = { value: ko.observable() }
		model = new SettingsViewModel(searchModel)
		model.loader 0
		model.enableSmartOrder true
		searchModel.value "DbfSource"
		equal $("#drugstore_SmartOrderRules_CodeColumn").rules().required, true
		equal $("#drugstore_SmartOrderRules_QuantityColumn").rules().required, true
		equal $("#drugstore_SmartOrderRules_AssortimentPriceCode_Id").rules().required, true
