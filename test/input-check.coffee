$ ->
	test "Check input text to forbidden symbols", ->
		ok CheckOnForbiddenSymbols("This text not contains forbidden symbols")
		equal CheckOnForbiddenSymbols("<This text contains forbidden symbols"), false
		equal CheckOnForbiddenSymbols("<> This <te.xt conta>in!@@##s forbid$#den sy$%^mbols"), false
