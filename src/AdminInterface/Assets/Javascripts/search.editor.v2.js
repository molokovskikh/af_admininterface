SearchViewModel = function (el) {
	var self = this;
	self.title = el.attr("data-search-title");

	var hidden = el.find("input[type=hidden]");
	self.label = hidden.data("label");
	self.value = ko.observable(hidden.val());
	self.text = ko.observable(hidden.data("text"));
	self.originValue = self.value();
	self.originText = self.text();

	self.url = window.searchEditors[el.attr("data-search-editor")].url;
	self.templatePrefix = "search-editor-template-";
	self.template = ko.observable();
	self.term = ko.observable();
	self.result = ko.observableArray([]);
	self.error = ko.observable();
	self.message = ko.observable();
	self.canSearch = ko.observable(true);
	if (self.value())
		self.template(self.templatePrefix + "value");
	else
		self.template(self.templatePrefix + "search");
	self.value.subscribe(function () {
		var data = self.result();
		if (data) {
			var item = _.find(data, function (i) { return i.id == self.value(); });
			if (item)
				self.text(item.name);
		}
	});
	self.search = function () {
		self.error("");
		$.ajax({
			url: self.url,
			data: { "text": self.term() },
			cache: false,
			success: function (data) {
				if (data.length == 0) {
					self.error("Ничего не найдено.");
					return;
				}
				self.result(data);
				self.template(self.templatePrefix + "select");
			},
			error: function (xhr, textStatus, error) {
				self.error("Произошла ошибка. Попробуйте еще раз.");
			},
			complete: function () {
				self.canSearch(true);
				self.message("");
			}
		});
		self.canSearch(false);
		self.message("Идет поиск...");
	};
	self.edit = function () {
		self.template(self.templatePrefix + "search");
		self.value("");
		self.text("");
	};
	self.cancel = function () {
		if (self.originValue) {
			self.template(self.templatePrefix + "value");
			self.value(self.originValue);
			self.text(self.originText);
		}
	};
	return self;
};

$(function () {
	$(".search-editor-v2").each(function () {
		var el = $(this);
		var model = new SearchViewModel(el);
		el.data("model", model);
		ko.applyBindings(model, this);
	});
});
