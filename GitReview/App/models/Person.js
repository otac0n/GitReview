App.Person = DS.Model.extend({
    name: DS.attr('string')
});

App.PersonSerializer = DS.RESTSerializer.extend({
    primaryKey: 'email'
});
