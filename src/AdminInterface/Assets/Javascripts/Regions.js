function ShowDefaultRegions() {
	if (jQuery("#ShowRegionsLink")[0].innerHTML == "Скрыть") {
		$('#EditDefaultRegions').hide();
		jQuery("#ShowRegionsLink")[0].innerHTML = "Редактировать регионы работы по умолчанию";
	}
	else {
		$('#EditDefaultRegions').show();
		jQuery("#ShowRegionsLink")[0].innerHTML = "Скрыть";
	}
}