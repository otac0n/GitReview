App.Commit = DS.Model.extend({
    message: DS.attr('string'),
    author: DS.belongsTo('person'),
    authoredAt: DS.attr('date'),
    committer: DS.belongsTo('person'),
    committedAt: DS.attr('date'),
    parents: DS.hasMany('commit')
});
