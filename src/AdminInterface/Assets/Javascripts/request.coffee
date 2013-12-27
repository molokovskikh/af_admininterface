requests = Object()

this.fillDependedData = (url, element, next) ->
	request = element.attr("data-request")
	requestFunction = requests[request]
	showRequest = (element.prop("checked") and requestFunction and not element.attr("unchecked")) or (not element.prop("checked") and requestFunction and element.attr("unchecked"))
	if showRequest
		cancel = ->
			if not element.attr("unchecked")
				element.prop("checked", false)
			else
				element.prop("checked", true)
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
				params = url
				url = $(this).children("div").children("input").each ->
					params += "&" + $.param $(this)
				$(this).dialog("destroy")
				next(params)
			"Отменить": ->
				cancel()
				$(this).dialog("destroy")
	form.validate()

freePeriodEnd = (url, next, cancel) ->
	form = $("<form><div><label>Дата окончания бесплатного периода</label><input name=FreePeriodEnd class='date'> </br>" +
	"<label>Основание бесплатного обслуживания</label><input id='AddCommentField' name='AddComment' class='required' >" +
	"</div></form>")
	showForm(url, next, cancel, form)

addComment = (url, next, cancel) ->
	form = $("<form onsubmit='return false;'><div><label>Введите причину отключения</label><input id='AddCommentField' name='AddComment' class='required' ></div></form>")
	showForm(url, next, cancel, form)

requests["FreePeriodEnd"] = freePeriodEnd
requests["AddComment"] = addComment
