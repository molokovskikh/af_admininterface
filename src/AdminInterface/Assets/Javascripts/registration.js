function RegistrationViewModel() {
	var self = this;
	self.forSupplier = ko.observable(false);
	self.isHiddenFromSupplier = ko.observable(false);
	self.isHiddenFromSupplierEnable = ko.computed(function () {
		return !self.forSupplier();
	}, self);
	self.forSupplier.subscribe(function (value) {
		self.isHiddenFromSupplier(value);
	});
}