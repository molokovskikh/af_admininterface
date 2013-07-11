define ["smart-order-settings", "knockout"], (m, ko) ->
	window.ko = ko
	module "settings",
		setup: ->
			$("form").validate()
	test "set validation rules", ->
		searchModel = { value: ko.observable("DbfSource") }
		model = new SettingsViewModel(searchModel)
		model.loader 0
		model.enableSmartOrder true
		searchModel.value "DbfSource"
		equal $("#drugstore_SmartOrderRules_CodeColumn").rules().required, true
		equal $("#drugstore_SmartOrderRules_QuantityColumn").rules().required, true
		equal $("#drugstore_SmartOrderRules_AssortimentPriceCode_Id").rules().required, true

	test "disable validation if source not cofigurable", ->
		searchModel = { value: ko.observable("TestSource") }
		model = new SettingsViewModel(searchModel)
		model.loader 0
		model.enableSmartOrder true
		equal $("#drugstore_SmartOrderRules_CodeColumn").rules().required, undefined

	test "update validation on parser change", ->
		searchModel = { value: ko.observable("TestSource") }
		model = new SettingsViewModel(searchModel)
		model.loader 0
		model.enableSmartOrder true
		searchModel.value("DbfSource")
		equal $("#drugstore_SmartOrderRules_CodeColumn").rules().required, true
		searchModel.value("TestSource")
		equal $("#drugstore_SmartOrderRules_CodeColumn").rules().required, undefined
