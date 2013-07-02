window.SettingsViewModel = function (searchModel) {
	var self = this;
	var elPrefix = "#drugstore_SmartOrderRules_";
	var propertyMap = {
		"ColumnSeparator": ["TextSource"],
		"StartLine": ["TextSource", "ExcelSource"],
		"CodePage": ["TextSource", "DbfSource"]
	};
	//настройки валидации в зависимости от разных наборов опция могут быть разные настройки
	var requiredFields = {
		0: ["CodeColumn"],
		1: ["CodeColumn", "CodeCrColumn"],
		2: ["ProductColumn"],
		3: ["ProductColumn", "ProducerColumn"],
	};
	var configurableParsers = ["ExcelSource", "DbfSource", "TextSource"];
	self.lastLoaderValue = null;
	self.parser = ko.computed(function () {
		return searchModel.value();
	}, searchModel);
	self.enableSmartOrder = ko.observable();
	self.enableSmartOrder.subscribe(function () {
		if (self.enableSmartOrder()) {
			$(elPrefix + "AssortimentPriceCode_Id").rules("add", "required");
			$(elPrefix + "ParseAlgorithm").rules("add", "required");
		} else {
			$(elPrefix + "AssortimentPriceCode_Id").rules("remove", "required");
			$(elPrefix + "ParseAlgorithm").rules("remove", "required");
		}
	});
	self.loader = ko.observable();
	self.updateLoaderValidation = function () {
		self.updateRules(self.lastLoaderValue, "remove");
		if (self.enableSmartOrder())
			self.updateRules(self.loader(), "add");
		lastLoaderValue = self.loader();
	};
	self.loader.subscribe(self.updateLoaderValidation);
	self.enableSmartOrder.subscribe(self.updateLoaderValidation);
	self.updateRules = function(value, action) {
		var rules = requiredFields[value];
		if (rules) {
			$.each(rules, function (i, v) {
				$(elPrefix + v).rules(action, "required");
			});
		}
	};
	self.isConfigurable = ko.computed(function () {
		return self.enableSmartOrder() && configurableParsers.indexOf(self.parser()) >= 0;
	}, self);
	self.isConfigurable.subscribe(function () {
		if (self.isConfigurable())
			$(elPrefix + "QuantityColumn").rules("add", "required");
		else
			$(elPrefix + "QuantityColumn").rules("remove", "required");
	});
	self.canConfigure = function (name) {
		var parsers = propertyMap[name];
		if (parsers)
			return parsers.indexOf(self.parser()) >= 0;
		return true;
	};
	self.isStartLineAvailable = ko.computed(function () {
		return self.canConfigure("StartLine");
	});
	self.isCodePageAvailable = ko.computed(function () {
		return self.canConfigure("CodePage");
	});
	self.isColumnSeparatorAvailable = ko.computed(function () {
		return self.canConfigure("ColumnSeparator");
	});
	self.isColumnSeparatorAvailable.subscribe(function () {
		if (self.isColumnSeparatorAvailable())
			$(elPrefix + "ColumnSeparator").rules("add", "required");
		else
			$(elPrefix + "ColumnSeparator").rules("remove", "required");
	});
	//нужно что бы отработал метод который был зарегистрирован выше
	self.isColumnSeparatorAvailable.notifySubscribers();
	self.isConfigurable.notifySubscribers();
};

if (!(typeof require === 'function')) {
	$(function () {
		ko.bindingHandlers.allowBindings = {
			init: function (elem, valueAccessor) {
				var shouldAllowBindings = ko.utils.unwrapObservable(valueAccessor());
				return { controlsDescendantBindings: !shouldAllowBindings };
			}
		};

		$("#DrugstoreSettingsForm").validate();
		var searchModel = ko.dataFor($("#drugstore_SmartOrderRules_ParseAlgorithm").get(0));
		var settingsModel = new SettingsViewModel(searchModel);
		ko.applyBindings(settingsModel);
	});
}
