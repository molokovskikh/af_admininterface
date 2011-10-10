window.Region = Backbone.Model.extend()
window.Regions = Backbone.Collection.extend(model: Region)

window.Group = Backbone.Model.extend
	initialize: ->
		this.region = this.attributes.region

window.GroupView = Backbone.View.extend
	render: ->
		@el = $("#orderDeliveryGroup").tmpl(@model.region)
		@el

window.GroupListView = Backbone.View.extend
	model: Group

	initialize: ->
		regions = this.options.regions
		regions.bind "add", this.add, this
		regions.bind "remove", this.remove, this
		this.el = $ "#groups"

	remove: (region) ->
		$(region.group.view.el).remove()

	add: (region) ->
		group = new Group(region: region)
		group.view = new GroupView(model: group)
		region.group = group
		$(this.el).append group.view.render()
