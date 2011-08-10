(function() {
  $(function() {
    test("fire add event", function() {
      var added, regions;
      regions = new Regions([
        {
          name: "test"
        }
      ]);
      added = null;
      regions.bind("add", function(region) {
        return added = region;
      });
      regions.add({
        name: "test1"
      });
      return equal(added.get("name"), "test1");
    });
    test("add regional delivery group on region add", function() {
      var element, view;
      $("#groups").children().remove();
      window.regions = new Regions([
        {
          name: "test"
        }
      ]);
      view = new GroupListView({
        regions: regions
      });
      regions.add({
        name: "test1"
      });
      element = $("#groups").children().get(0);
      return equals($(element).html(), "test1");
    });
    return test("remove group on region delete", function() {
      var view;
      $("#groups").children().remove();
      window.regions = new Regions([
        {
          name: "test"
        }
      ]);
      view = new GroupListView({
        regions: regions
      });
      regions.add({
        name: "test1"
      });
      equal($("#groups").children().length, 1);
      regions.remove(regions.at(1));
      return equal($("#groups").children().length, 0);
    });
  });
}).call(this);
