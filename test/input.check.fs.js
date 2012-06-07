(function() {
	$(function() {
		return test("Check input text to forbidden symbols", function() {
			ok(CheckForForbiddenSymbols("This text not contains forbidden symbols"));
			equal(CheckForForbiddenSymbols("<This text contains forbidden symbols"), false);
			equal(CheckForForbiddenSymbols("<> This <te.xt conta>in!@@##s forbid$#den sy$%^mbols"), false);
		});
	});
}).call(this);