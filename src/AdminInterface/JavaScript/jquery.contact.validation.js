// Валидаторы для проверки корректности введенного номера телефона и email
// чтобы их применить, нужно у input-a выставить значение атрибута class в "email" или "phone"
// и при загрузке документа вызвать метод validate у формы
$(function () {
	$.validator.addMethod("phone", function (value, element) {
		if (value.toString().length > 0) {
			return /^(\d{3})-(\d{7})(\*\d{3})?$/.test(value) || /^(\d{4})-(\d{6})(\*\d{3})?$/.test(value);
		}
		return true;
	}, "Некорректный телефонный номер, номер должен быть указан в формате xxx-xxxxxx.");

	// Валидатор для внутреннего номера
	$.validator.addMethod("InternalPhone", function (value, element) {
		if (value.toString().length > 0) {
			return /^(\d{3})$/.test(value);
		}
		return true;
	}, "Некорректный телефонный номер, номер должен быть указан в формате xxx.");

	// Валидатор для внутреннего номера
	$.validator.addMethod("date", function (value, element) {
		if (value.toString().length > 0) {
			return this.optional(element) || (/^(\d{1,2}\.\d{1,2}.\d{2,4})$/.test(value) && Date.parse(value));
		}
		return true;
	}, "Пожалуйста, введите корректную дату, в формате дд.ММ.гггг.");

	$.validator.addMethod("mobile-phone", function (v) {
		return /^\d{10}$/.test(v);
	}, "Телефон должен быть десятизначный");

	$.validator.addMethod("email", function (value, element) {
		if (value.toString().length > 0) {
			return /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/.test(value);
		}
		return true;
	}, "Некорректный адрес электронной почты.");

	$.validator.addMethod("validateEmailList", function (value, element) {
		if (value.toString().length > 0) {
			return /^\s*\w[\w\.\-]*[@]\w[\w\.\-]*([.]([\w]{1,})){1,3}\s*(\,\s*\w[\w\.\-]*[@]\w[\w\.\-]*([.]([\w]{1,})){1,3}\s*)*$/.test(value);
		}
		return true;
	}, "Поле содержит некорректный адрес электронной почты.");

	$.validator.addMethod("validateForbiddenSymbols", function (value, element) {
		return CheckOnForbiddenSymbols(value);
	}, "Поле содержит запрещенные символы(<, >).");
});

function CheckOnForbiddenSymbols(checkedString) {
	if (checkedString.toString().length > 0) {
		return /^[^<>]*$/.test(checkedString);
	}
	return true;
}