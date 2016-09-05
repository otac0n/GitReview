function compareCommits(a, b) {
    var comp;
    if ((comp = a.get('committedAt') - b.get('committedAt')) != 0) return comp;
    if ((comp = a.get('authoredAt') - b.get('authoredAt')) != 0) return comp;
    if ((comp = a.id.localeCompare(b.id)) != 0) return comp;
    return 0;
}

App.CommitGridController = Ember.Controller.extend({
    rows: function () {
        var source = this.model.get('source');
        var destination = this.model.get('destination');

        var terminal = {};
        var ignored = {};
        var mergeBases = {};
        this.model.get('mergeBases').forEach(function (m) {
            terminal[m.id] = true;
            mergeBases[m.id] = true;
        });
        this.model.get('ignored').forEach(function (i) {
            terminal[i.id] = true;
            ignored[i.id] = true;
        });

        function getId(commit) {
            return commit.id;
        }

        function getParents(commit) {
            var parents = [];

            if (commit == null) {
                parents.push(destination);
                parents.push(source);
            } else if (!terminal[commit.id]) {
                var parentsArray = commit.get('parents');
                parentsArray.forEach(function (p) {
                    if (!ignored[p.id]) {
                        parents.push(p);
                    }
                });
            }

            return parents;
        }

        var canvas = document.createElement('canvas');
        var rows = new DagRenderer({ rowHeight: 12, colWidth: 7, margin: 6 }).render(
            getId,
            getParents,
            compareCommits,
            canvas);

        return rows;
    }.property('model.source', 'model.destination', 'model.mergeBases', 'model.ignored')
});
