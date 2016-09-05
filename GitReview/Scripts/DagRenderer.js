// -----------------------------------------------------------------------
// <copyright file="DagRenderer.js" company="(none)">
//   Copyright © 2015 John Gietzen.  All Rights Reserved.
//   This source is subject to the MIT license.
//   Please see license.md for more information.
// </copyright>
// -----------------------------------------------------------------------

DagRenderer = (function () {
    var defaults = {
        colWidth: 13,
        rowHeight: 24,
        lineWidth: 2,
        curveLine: true,
        dotRadius: 3,
        dotBorder: 0,
        margin: 10,
        rightAlign: true,
        flushLeft: false,
        usePalette: true,
        palette: [
            "0061B0",
            "911822",
            "CCAD49",
            "439959",
            "A01E86",
            "875B0E",
            "EA4517",
            "2B14AD",
            "3E6000",
            "68727F",
            "000000"
        ]
    };

    function indexOf(array, predicate) {
        for (var i = 0; i < array.length; i++) {
            if (predicate(array[i])) return i;
        }
        return -1;
    }

    function findSatisfied(nodes, satisfied) {
        return indexOf(nodes, function (node) {
            for (var i = 0; i < node.parents.length; i++) {
                if (!satisfied[node.parents[i]]) return false;
            }
            return true;
        });
    }

    function Graph() { }

    Graph.prototype.getNodes = function getNodes(comparefn) {
        var nodes = [];
        for (var id in this) {
            if (this.hasOwnProperty(id)) {
                nodes.push(this[id]);
            }
        }

        nodes.sort(comparefn);

        var satisfied = {};
        var topoOrder = [];
        while (nodes.length > 0) {
            var ix = findSatisfied(nodes, satisfied);

            if (ix < 0) throw new "Assertion failed.";

            var node = nodes.splice(ix, 1)[0];
            topoOrder.unshift(node);
            satisfied[node.id] = true;
        }

        return topoOrder;
    }

    function getGraph(getId, getParents) {
        var graph = new Graph();

        var queue = getParents(null);

        while (queue.length > 0) {
            var element = queue.pop();
            var id = getId(element);

            if (!graph[id]) {
                var parents = getParents(element);
                var parentIds = [];

                for (var i = 0; i < parents.length; i++) {
                    var p = parents[i];
                    queue.push(p);
                    parentIds.push(getId(p));
                }

                var node = { id: id, element: element, parents: parentIds };
                graph[id] = node;
            }
        }

        return graph;
    }

    function buildOutgoing(incoming, node) {
        var outgoing = incoming.slice(0);

        var col = outgoing.indexOf(node.id);
        if (col == -1) {
            col += outgoing.push(node.id);
        }

        var replaced = false;
        for (var p = 0; p < node.parents.length; p++) {
            var parent = node.parents[p];
            if (outgoing.indexOf(parent) == -1) {
                if (!replaced) {
                    outgoing[col] = parent;
                    replaced = true;
                } else {
                    outgoing.push(parent);
                }
            }
        }

        if (!replaced) {
            outgoing.splice(col, 1);
        }

        return outgoing;
    }

    function attachIncomingOutgoing(nodes) {
        var incoming = [];
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];

            var outgoing = buildOutgoing(incoming, node);

            node.incoming = incoming;
            incoming = node.outgoing = outgoing;
        }
    }

    function colorNodes(nodes) {
        var colors = {};

        var nextColor = 0;
        for (var i = 0; i < nodes.length; i++) {
            var node = nodes[i];

            var color = colors[node.id];
            if (color === undefined) {
                color = colors[node.id] = nextColor++;
            }

            var transferred = false;
            for (var p = 0; p < node.parents.length; p++) {
                var parent = node.parents[p];
                if (!colors[parent]) {
                    if (!transferred) {
                        colors[parent] = color;
                        transferred = true;
                    } else {
                        colors[parent] = nextColor++;
                    }
                }
            }
        }

        return colors;
    }

    function makeShapes(nodes, colors) {
        var shapes = [];
        for (var row = 0; row < nodes.length; row++) {
            var node = nodes[row];
            var incoming = node.incoming;
            var outgoing = node.outgoing;
            var parents = node.parents;

            var col = incoming.indexOf(node.id);
            if (col == -1) {
                incoming = incoming.slice(0);
                col += incoming.push(node.id);
            }

            for (var i = 0; i < incoming.length; i++) {
                var o = outgoing.indexOf(incoming[i]);
                if (o != -1) {
                    shapes.push({ type: "connection", start: { x: i, y: row }, end: { x: o, y: row + 1 }, color: colors[incoming[i]] });
                }
            }

            for (var p = parents.length - 1; p >= 0; p--) {
                var pCol = outgoing.indexOf(parents[p]);
                if (pCol != -1) {
                    shapes.push({ type: "connection", start: { x: col, y: row }, end: { x: pCol, y: row + 1 }, color: colors[parents[p]] });
                }
            }

            shapes.push({ type: "circle", center: { x: col, y: row }, color: colors[node.id] });
        }

        return shapes;
    }

    function drawShapes(shapes, canvas, options) {
        var maxWidth = 1;
        var maxHeight = 1;
        for (var i = 0; i < shapes.length; i++) {
            var locations = [shapes[i].start, shapes[i].end, shapes[i].center];
            for (var l = 0; l < locations.length; l++) {
                var location = locations[l];
                if (location) {
                    maxWidth = Math.max(maxWidth, location.x + 1);
                    maxHeight = Math.max(maxHeight, location.y + 1);
                }
            }
        }

        var context = canvas.getContext("2d");
        canvas.width = options.margin * 2 + maxWidth * options.colWidth;
        canvas.height = options.margin * 2 + (maxHeight - 1) * options.rowHeight;

        var mapColor = function (num) {
            return "#" + options.palette[num % options.palette.length];
        };

        var map = function (location) {
            return {
                x: options.margin + (options.rightAlign ? maxWidth - location.x - 1 : location.x) * options.colWidth,
                y: options.margin + location.y * options.rowHeight
            };
        };

        for (var i = 0; i < shapes.length; i++) {
            if (shapes[i].type == "connection") {
                var start = map(shapes[i].start);
                var end = map(shapes[i].end);
                var color = mapColor(shapes[i].color);
                context.beginPath();
                context.moveTo(start.x, start.y);
                if (options.curveLine) {
                    context.bezierCurveTo(start.x, end.y - options.rowHeight / 2, end.x, start.y + options.rowHeight / 2, end.x, end.y);
                } else {
                    context.lineTo(end.x, end.y);
                }
                context.strokeStyle = color;
                context.lineWidth = options.lineWidth;
                context.stroke();
            } else if (shapes[i].type == "circle") {
                var center = map(shapes[i].center);
                var color = mapColor(shapes[i].color);
                context.beginPath();
                context.arc(center.x, center.y, options.dotRadius, 0, 2 * Math.PI, false);
                context.fillStyle = color;
                context.fill();
                if (options.dotBorder) {
                    context.strokeStyle = "#000";
                    context.lineWidth = options.dotBorder;
                    context.stroke();
                }
            }
        }
    }

    function render(getId, getParents, comparefn, canvas) {
        var graph = getGraph(getId, getParents);
        var nodes = graph.getNodes(function (a, b) { return comparefn(a.element, b.element); });
        attachIncomingOutgoing(nodes);
        var colors = colorNodes(nodes);
        var shapes = makeShapes(nodes, colors);
        drawShapes(shapes, canvas, this.options);
        return nodes;
    }

    function DagRenderer(options) {
        this.options = $.extend({}, defaults, options);
    }
    DagRenderer.prototype.render = render;
    return DagRenderer;
})();
