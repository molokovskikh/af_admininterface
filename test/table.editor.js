(function() {
  $(function() {
    test("create new row in empty table", function() {
      $("#empty a.new").click();
      return equal($("#empty tbody tr").length, 1);
    });
    return test("create new row in not empty table", function() {
      $("#row1 a.new").click();
      return equal($("#row1 tbody tr").length, 2);
    });
  });
}).call(this);
