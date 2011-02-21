// Валидаторы для проверки корректности введенного номера телефона и email
// чтобы их применить, нужно у input-a выставить значение атрибута class в "email" или "phone"
// и при загрузке документа вызвать метод validate у формы
jQuery(function() {
	jQuery.validator.addMethod("phone", function(value, element) {
            if (value.toString().length > 0) {
                res = /^(\d{3,4})-(\d{6,7})(\*\d{3})?$/.test(value)
                return res;
            }
            return true;
        }, "Некорректный телефонный номер");
		
	// Валидатор для внутреннего номера
	jQuery.validator.addMethod("InternalPhone", function(value, element) {
            if (value.toString().length > 0) {
                res = /^(\d{3})$/.test(value)
                return res;
            }
            return true;
        }, "Некорректный телефонный номер");		

        jQuery.validator.addMethod("email", function(value, element) {
            if (value.toString().length > 0) {
                res = /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/.test(value)
                return res;
            }
            return true;
        }, "Некорректный адрес электронной почты");  
});