App.Commit = DS.Model.extend({
    message: DS.attr('string'),
    author: DS.belongsTo('person'),
    committer: DS.belongsTo('person'),
    parents: DS.hasMany('commit')
});
