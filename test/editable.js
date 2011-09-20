(function() {
  $(function() {
    return test("add row into editable table", function() {
      $("#with_thead").data("template", function() {
        return $("<tr></tr>");
      });
      $("#with_thead a.add").click();
      return equal($("#with_thead tbody tr").length, 2);
    });
  });
}).call(this);
