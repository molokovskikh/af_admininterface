requests = Object()

this.fillDependedData = (url, element, next) ->
	request = element.attr("data-request")
	requestFunction = requests[request]
	showRequest = (element.attr("checked") and requestFunction and not element.attr("unchecked")) or (not element.attr("checked") and requestFunction and element.attr("unchecked"))
	if showRequest
		cancel = ->
			if not element.attr("unchecked")
				element.removeAttr("checked")
			else
				element.attr("checked", true)
			element.change()
		requestFunction(url, next, cancel)
	else
		next(url)

showForm = (url, next, cancel, form) ->
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

freePeriodEnd = (url, next, cancel) ->
	form = $("<form><div><label>Дата окончания бесплатного периода</label><input name=FreePeriodEnd class='date'></div></form>")
	showForm(url, next, cancel, form)

addComment = (url, next, cancel) ->
	form = $("<form><div><label>Введите комментарий</label><input name=AddComment ></div></form>")
	showForm(url, next, cancel, form)

requests["FreePeriodEnd"] = freePeriodEnd
requests["AddComment"] = addComment
