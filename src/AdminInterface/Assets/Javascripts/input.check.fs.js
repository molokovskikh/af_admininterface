function CheckForForbiddenSymbols(checkedString) {
	if (checkedString.toString().length > 0) {
		return /^[^<>]*$/.test(checkedString);
	}
	return true;
}