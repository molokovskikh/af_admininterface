requests = Object()

this.fillDependedData = (url, element, next) ->
	request = element.attr("data-request")
	requestFunction = requests[request]
	showRequest = element.attr("checked") and requestFunction
	if showRequest
		cancel = ->
			element.removeAttr("checked")
			element.change()
		requestFunction(url, next, cancel)
	else
		next(url)

freePeriodEnd = (url, next, cancel) ->
	form = $("<form><div><label>Дата окончания бесплатного периода</label><input name=FreePeriodEnd class='date'></div></form>")
	form.dialog
		modal: true
		buttons:
			"Продолжить": ->
				return unless $(this).valid()
				url += "&" + $.param $(this).find("input")
				$(this).dialog("destroy")
				next(url)
			"Отменить": ->
				cancel()
				$(this).dialog("destroy")
	form.validate()

requests["FreePeriodEnd"] = freePeriodEnd
