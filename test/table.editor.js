(function() {
  $(function() {
    test("create new row in empty table", function() {
      $("#empty a.new").click();
      return equal($("#empty tbody tr").length, 1);
    });
    test("create new row in not empty table", function() {
      $("#row1 a.new").click();
      return equal($("#row1 tbody tr").length, 2);
    });
    return test("increment index in name", function() {
      $("#row1 a.new").click();
      equal($("#row1 input").get(0).name, "items[0].Name");
      return equal($("#row1 input").get(1).name, "items[1].Name");
    });
  });
}).call(this);
