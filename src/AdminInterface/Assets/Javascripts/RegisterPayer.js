$(function () {

	if ($('#PaymentOptions_WorkForFree') != null) {
		$('#PaymentOptions_WorkForFree').change(function () {

			if (this.checked) {
				$('#PaymentOptions_PaymentPeriodBeginDate').attr('disabled', 'disabled');
				$('#PaymentOptions_Comment').attr('disabled', 'disabled');
			}
			else {
				$('#PaymentOptions_PaymentPeriodBeginDate').removeAttr('disabled');
				$('#PaymentOptions_Comment').removeAttr('disabled');
			}
		});
	}

	$.validator.addMethod("CommentValidator", function (value, element, params) {
		if ($('PaymentOptions_WorkForFree').prop("checked"))
			return true;

		if (!$("PaymentOptions_Comment").empty())
			return true;

		var maxFreeWorkWithoutMessage = Date.today().add(45).days();
		var paymentPeriodBeginDate = Date.parse($('#PaymentOptions_PaymentPeriodBeginDate').val());
		return paymentPeriodBeginDate < maxFreeWorkWithoutMessage;
	}, "Клиент слишком долго работает бесплатно, нужно указать причину.");

	$.validator.addMethod("DateInFuture", function (value, element, params) {
		if ($('PaymentOptions_WorkForFree').prop("checked"))
			return true;

		var paymentPeriodBeginDate = Date.parse($('#PaymentOptions_PaymentPeriodBeginDate').val());
		return paymentPeriodBeginDate >= Date.today();
	}, "Дата начала платного периода меньше текущей.");

	$('#PayerInfo').validate();

	$('#PaymentOptions_PaymentPeriodBeginDate').rules("add", {
		date: true, DateInFuture: true,
		messages: {
			date: "Ошибка ввода даты"
		}
	});

	$('#Instance_Name').rules("add", {
		required: true,
		messages: {
			required: "Введите краткое наименование"
		}
	});

	$('#PaymentOptions_Comment').rules("add", {
		CommentValidator: true
	});
});