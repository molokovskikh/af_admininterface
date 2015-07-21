function RegistrationViewModel() {
	var self = this;
	self.orgs = ko.observableArray([]);
	self.showOrgsEdit = ko.computed(function () {
		return self.orgs().length > 1;
	}, self);
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
		var parsedValue = parseInt(value);
		if (!parsedValue) {
			self.orgs([]);
			return;
		}
		$.get("/Clients/GetPayerOrgs",
			{ id: parsedValue },
			function (data) {
				self.orgs(data);
			});
	});
}