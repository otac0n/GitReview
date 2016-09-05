App.Revision = DS.Model.extend({
    source: DS.belongsTo('commit'),
    destination: DS.belongsTo('commit'),
    mergeBases: DS.hasMany('commit'),
    ignored: DS.hasMany('commit'),
});
