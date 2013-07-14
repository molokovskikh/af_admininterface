(function() {
  var registerFilter;
  registerFilter = function() {
    var allLabel, currentLabel, filter, unfilter;
    allLabel = "Показать для всех";
    currentLabel = "Показать только для текущего";
    $(".filter a[data-filter]").click(function() {
      var currentFilter;
      currentFilter = $(".current-filter");
      if (currentFilter.length) {
        unfilter();
        currentFilter.removeClass("current-filter");
      }
      $(this).addClass("current-filter");
      $(".filter-checker").text(allLabel).show();
      return filter();
    });
    $(".filter-checker").click(function() {
      if ($(this).text() === allLabel) {
        unfilter();
        return $(this).text(currentLabel);
      } else {
        filter();
        return $(this).text(allLabel);
      }
    });
    filter = function() {
      var filterValue;
      filterValue = $(".current-filter").attr("data-filter");
      return $(".filtrable tbody tr[data-filter!=" + filterValue + "]").hide();
    };
    return unfilter = function() {
      return $(".filtrable tr").show();
    };
  };
  window.registerFilter = registerFilter;
  if (typeof require !== 'function') {
    $(function() {
      return registerFilter();
    });
  }
}).call(this);
