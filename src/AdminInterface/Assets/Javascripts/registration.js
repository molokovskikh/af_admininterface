function RegistrationViewModel() {
	var self = this;
	self.orgs = ko.observableArray([]);
	self.payerId = ko.observable();
	self.forSupplier = ko.observable(false);
	self.isHiddenFromSupplier = ko.observable(false);
	self.isHiddenFromSupplierEnable = ko.computed(function () {
		return !self.forSupplier();
	}, self);
	self.forSupplier.subscribe(function (value) {
		self.isHiddenFromSupplier(value);
	});
	self.payerId.subscribe(function (value) {
		if (!parseInt(value)) {
			self.orgs([]);
			return;
		}
		$.get("../Clients/GetPayerOrgs",
			{ id: value },
			function (data) {
				self.orgs(data);
			});
	});
}