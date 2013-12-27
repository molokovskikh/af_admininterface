(function() {
  window.Region = Backbone.Model.extend();

  window.Regions = Backbone.Collection.extend({
    model: Region
  });

  window.Group = Backbone.Model.extend({
    initialize: function() {
      return this.region = this.attributes.region;
    }
  });

  window.GroupView = Backbone.View.extend({
    render: function() {
      this.el = $("#orderDeliveryGroup").tmpl(this.model.region);
      return this.el;
    }
  });

  window.GroupListView = Backbone.View.extend({
    model: Group,
    initialize: function() {
      var regions;
      regions = this.options.regions;
      regions.bind("add", this.add, this);
      regions.bind("remove", this.remove, this);
      return this.el = $("#groups");
    },
    remove: function(region) {
      return $(region.group.view.el).remove();
    },
    add: function(region) {
      var group;
      group = new Group({
        region: region
      });
      group.view = new GroupView({
        model: group
      });
      region.group = group;
      return $(this.el).append(group.view.render());
    }
  });

}).call(this);
