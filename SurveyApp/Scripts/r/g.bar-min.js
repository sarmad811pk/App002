/*!
 * g.Raphael 0.51 - Charting library, based on Raphaël
 *
 * Copyright (c) 2009-2012 Dmitry Baranovskiy (http://g.raphaeljs.com)
 * Licensed under the MIT (http://www.opensource.org/licenses/mit-license.php) license.
 */
(function () {
    var mmin = Math.min,
        mmax = Math.max;

    function finger(x, y, width, height, dir, ending, isPath, paper) {
        var path,
            ends = { round: 'round', sharp: 'sharp', soft: 'soft', square: 'square' };

        // dir 0 for horizontal and 1 for vertical
        if ((dir && !height) || (!dir && !width)) {
            return isPath ? "" : paper.path();
        }

        ending = ends[ending] || "square";
        height = Math.round(height);
        width = Math.round(width);
        x = Math.round(x);
        y = Math.round(y);

        switch (ending) {
            case "round":
                if (!dir) {
                    var r = ~~(height / 2);

                    if (width < r) {
                        r = width;
                        path = [
                            "M", x + .5, y + .5 - ~~(height / 2),
                            "l", 0, 0,
                            "a", r, ~~(height / 2), 0, 0, 1, 0, height,
                            "l", 0, 0,
                            "z"
                        ];
                    } else {
                        path = [
                            "M", x + .5, y + .5 - r,
                            "l", width - r, 0,
                            "a", r, r, 0, 1, 1, 0, height,
                            "l", r - width, 0,
                            "z"
                        ];
                    }
                } else {
                    r = ~~(width / 2);

                    if (height < r) {
                        r = height;
                        path = [
                            "M", x - ~~(width / 2), y,
                            "l", 0, 0,
                            "a", ~~(width / 2), r, 0, 0, 1, width, 0,
                            "l", 0, 0,
                            "z"
                        ];
                    } else {
                        path = [
                            "M", x - r, y,
                            "l", 0, r - height,
                            "a", r, r, 0, 1, 1, width, 0,
                            "l", 0, height - r,
                            "z"
                        ];
                    }
                }
                break;
            case "sharp":
                if (!dir) {
                    var half = ~~(height / 2);

                    path = [
                        "M", x, y + half,
                        "l", 0, -height, mmax(width - half, 0), 0, mmin(half, width), half, -mmin(half, width), half + (half * 2 < height),
                        "z"
                    ];
                } else {
                    half = ~~(width / 2);
                    path = [
                        "M", x + half, y,
                        "l", -width, 0, 0, -mmax(height - half, 0), half, -mmin(half, height), half, mmin(half, height), half,
                        "z"
                    ];
                }
                break;
            case "square":
                if (!dir) {
                    path = [
                        "M", x, y + ~~(height / 2),
                        "l", 0, -height, width, 0, 0, height,
                        "z"
                    ];
                } else {
                    path = [
                        "M", x + ~~(width / 2), y,
                        "l", 1 - width, 0, 0, -height, width - 1, 0,
                        "z"
                    ];
                }
                break;
            case "soft":
                if (!dir) {
                    r = mmin(width, Math.round(height / 5));
                    path = [
                        "M", x + .5, y + .5 - ~~(height / 2),
                        "l", width - r, 0,
                        "a", r, r, 0, 0, 1, r, r,
                        "l", 0, height - r * 2,
                        "a", r, r, 0, 0, 1, -r, r,
                        "l", r - width, 0,
                        "z"
                    ];
                } else {
                    r = mmin(Math.round(width / 5), height);
                    path = [
                        "M", x - ~~(width / 2), y,
                        "l", 0, r - height,
                        "a", r, r, 0, 0, 1, r, -r,
                        "l", width - 2 * r, 0,
                        "a", r, r, 0, 0, 1, r, r,
                        "l", 0, height - r,
                        "z"
                    ];
                }
        }

        if (isPath) {
            return path.join(",");
        } else {
            return paper.path(path);
        }
    }

    /*\
     * Paper.vbarchart
     [ method ]
     **
     * Creates a vertical bar chart
     **
     > Parameters
     **
     - x (number) x coordinate of the chart
     - y (number) y coordinate of the chart
     - width (number) width of the chart (respected by all elements in the set)
     - height (number) height of the chart (respected by all elements in the set)
     - values (array) values
     - opts (object) options for the chart
     o {
     o type (string) type of endings of the bar. Default: 'square'. Other options are: 'round', 'sharp', 'soft'.
     o gutter (number)(string) default '20%' (WHAT DOES IT DO?)
     o vgutter (number)
     o colors (array) colors be used repeatedly to plot the bars. If multicolumn bar is used each sequence of bars with use a different color.
     o stacked (boolean) whether or not to tread values as in a stacked bar chart
     o to
     o stretch (boolean)
     o }
     **
     = (object) path element of the popup
     > Usage
     | r.vbarchart(0, 0, 620, 260, [76, 70, 67, 71, 69], {})
     \*/

    function VBarchart(paper, x, y, width, height, values, opts) {
        opts = opts || {};
        var chartinst = this,
            type = opts.type || "square",
            gutter = parseFloat(opts.gutter || "20%"),
            chart = paper.set(),
            bars = paper.set(),
            covers = paper.set(),
            covers2 = paper.set(),
            total = Math.max.apply(Math, values),
            stacktotal = [],
            multi = 0,
            colors = opts.colors || chartinst.colors,
            len = values.length;
        var labelStartPosX = x, labelStartPosY= y + height;

        if (Raphael.is(values[0], "array")) {
            total = [];
            multi = len;
            len = 0;

            for (var i = values.length; i--;) {
                bars.push(paper.set());
                total.push(Math.max.apply(Math, values[i]));
                len = Math.max(len, values[i].length);
            }

            if (opts.stacked) {
                for (var i = len; i--;) {
                    var tot = 0;

                    for (var j = values.length; j--;) {
                        tot += +values[j][i] || 0;
                    }

                    stacktotal.push(tot);
                }
            }

            for (var i = values.length; i--;) {
                if (values[i].length < len) {
                    for (var j = len; j--;) {
                        values[i].push(0);
                    }
                }
            }

            total = Math.max.apply(Math, opts.stacked ? stacktotal : total);
        }

        total = (opts.to) || total;
        total = (total==0?1:total);
        var barGroupWidth = width / (len * (100 + gutter) + gutter) * 100;
        var barwidth = width / (len * (100 + gutter) + gutter) * 100,
            barhgutter = barwidth * gutter / 100,
            barvgutter = opts.vgutter == null ? 20 : opts.vgutter,
            stack = [],
            X = x + barhgutter,
            Y = (height - 2 * barvgutter) / total;
        
        if (!opts.stretch) {
            barhgutter = Math.round(barhgutter);
            barwidth = Math.floor(barwidth);
        }
        labelStartPosX = labelStartPosX + x;
        !opts.stacked && (barwidth /= multi || 1);
        
        
        var labels;
        if (opts.labels) {
            labels = opts.labels.split('|');
        }
        paper.text(x + width / 2, 30, opts.title ? opts.title : "").attr({ font: "16px Arial", "font-weight": "bold" });

        var showValues = true;
        if (typeof opts.showValues != 'undefined') {
            showValues = opts.showValues;
        }
        var showValuesLabels = false;
        if (typeof opts.showValuesLabels != 'undefined') {
            showValuesLabels = opts.showValuesLabels;
        }
        
        for (var i = 0; i < len; i++) {
            stack = [];
            var noOfColumns = 0;
            for (var j = 0; j < (multi || 1) ; j++) {
                //if (typeof opts.excludeZeroValues != 'undefined') {
                //    if (opts.excludeZeroValues == true && Math.round((multi ? values[j][i] : values[i]) * Y) == 0) {
                //        continue;
                //    }
                //}
                noOfColumns += 1;
                var h = Math.round((multi ? values[j][i] : values[i]) * Y),
                    top = y + height - barvgutter - h,
                    bar = finger(Math.round(X + barwidth / 2), top + h, barwidth, h, true, type, null, paper).attr({ stroke: "none", fill: colors[multi ? j : i] });

          
                var val = multi ? values[j][i] : values[i];
                var val1 = 0;
                var tri;
                if (opts.markers) {
                    val1 = opts.markers[i];
                    var h1 = Math.round(opts.markers[i] * Y);
                    var top1 = y + height - barvgutter - h1;
                    if (val1 > 0) {
                        var flag1;
                        //tri = paper.image("res/i/bulGraph.png", X - 4 + barwidth / 2, top1, 8, 10).rotate(-90);
                        tri = paper.rect(X - 4 + barwidth / 2, top1, 8, 4).attr({stroke: "none", fill:  "rgba(255,0,0,.8)"});
                        tri.node.xx = X - 4 + barwidth / 2;
                        tri.node.yy = top1;
                        tri.node.vv = val1;
                        tri.node.v = val;
                        if (opts.showBubble == false) {
                            if (tri.node) {
                                $(tri.node).hover(function () { flag1 = paper.popup(this.xx + 5, this.yy,"Target\n"+this.vv).attr({ font: "11px Arial", 'text-anchor': 'middle' }); }, function () { flag1.hide().remove(); });
                            }
                        }
                        else {
                            if (tri.node) {
                                $(tri.node).hover(function () { flag1 = paper.popup(this.xx + 5, this.yy, "Benchmark: " + parseFloat(this.vv).formatMoney(0, '$') + "\nExpense: " + parseFloat(this.v).formatMoney(0, '$')); }, function () { flag1.hide().remove(); });
                            }
                        }

                    }
                }
                

                if (multi) {
                    bars[j].push(bar);
                } else {
                    bars.push(bar);
                }

                bar.y = top;
                bar.x = Math.round(X + barwidth / 2);
                bar.w = barwidth;
                bar.h = h;
                bar.value = multi ? values[j][i] : values[i];
                bar.value1 = val1;
                bar.node.id =  i;
                bar.node.val =  multi ? values[j][i] : values[i];
                bar.tri = tri;
                bar.color = colors[multi ? j : i];
                if (showValues == true) {
                    if (opts.markers) {
                        paper.text(bar.x, bar.y - 15, parseFloat(bar.value || "0").formatMoney(0, '$') + (bar.value1 != 0 ? " \n(" + (100 * parseFloat(bar.value || "0") / parseFloat(bar.value1 || "0")).toFixed(2) + "%)" : "")).attr({ font: "11px Arial", 'text-anchor': 'middle' });
                    }
                    else {
                        paper.text(bar.x, (bar.value < 0 ? bar.y + 10 : bar.y - 10), (bar.value==0 && opts.ShowZeroValueAs?"-":parseFloat(bar.value || "0").formatMoney(0, '$'))).attr({ font: "11px Arial", 'text-anchor': 'middle' });
                    }
                }
                else {

                    if (showValuesLabels) {
                        if (j==1)
                            var FinalValue = parseFloat(bar.value || "0");//.formatMoney(0, '$') + " K";
                        else
                            var FinalValue = parseFloat(bar.value || "0");
                        paper.text(bar.x, bar.y - 10, FinalValue).attr({ font: "11px Arial", 'text-anchor': 'middle' });
                    }                   

                    
                }
                if (!opts.stacked) {
                    X += barwidth;
                } else {
                    stack.push(bar);
                }
                if (bar.tri) { bar.tri.toFront(); }
            }
            
           
            //if (labels && opts.excludeZeroValues) {
            //    paper.text(X - (noOfColumns*(barwidth / 2)), y + height - 10, labels[i]).attr({ font: '11px Arial', 'text-anchor': 'middle' });
            //}
            if (opts.stacked) {
                var cvr;

                covers2.push(cvr = paper.rect(stack[0].x - stack[0].w / 2, y, barwidth, height).attr(chartinst.shim));
                cvr.bars = paper.set();

                var size = 0;

                for (var s = stack.length; s--;) {
                    stack[s].toFront();
                }

                for (var s = 0, ss = stack.length; s < ss; s++) {
                    var bar = stack[s],
                        cover,
                        h = (size + bar.value) * Y,
                        path = finger(bar.x, y + height - barvgutter - !!size * .5, barwidth, h, true, type, 1, paper);

                    cvr.bars.push(bar);
                    size && bar.attr({ path: path });
                    bar.h = h;
                    bar.y = y + height - barvgutter - !!size * .5 - h;
                    covers.push(cover = paper.rect(bar.x - bar.w / 2, bar.y, barwidth, bar.value * Y).attr(chartinst.shim));
                    cover.bar = bar;
                    cover.value = bar.value;
                    size += bar.value;
                }

                X += barwidth;
            }
            if (labels) { // && !opts.excludeZeroValues) {
                var curValue = (multi ? 0 : values[i]);
    
                //labelStartPosX = X + (multi ? (barGroupWidth / 2) : (barwidth / 2));
                labelStartPosX = (X - (noOfColumns * (multi ? (barGroupWidth / 2) : (barwidth / 2))));
                if (opts.stacked) {
                    labelStartPosX = (X - ((multi ? (barGroupWidth / 2) : (barwidth / 2))));
                }
                var yyy = (curValue < 0 ? labelStartPosY - 35 : labelStartPosY - 10);
                paper.text(labelStartPosX, yyy, labels[i]).attr({ font: '11px Arial', 'text-anchor': 'middle', fill: "#333333" });

            }
            X += barhgutter;
        }
        
        //covers2.toFront();
        X = x + barhgutter;

        if (!opts.stacked) {
            for (var i = 0; i < len; i++) {
                for (var j = 0; j < (multi || 1) ; j++) {
                    //var cover;

                    //covers.push(cover = paper.rect(Math.round(X), y + barvgutter, barwidth, height - barvgutter).attr(chartinst.shim));
                    //cover.bar = multi ? bars[j][i] : bars[i];
                    //cover.value = cover.bar.value;
                    //X += barwidth;
                }

                //X += barhgutter;
            }
        }

        chart.label = function (labels, isBottom) {
            labels = labels || [];
            this.labels = paper.set();

            var L, l = -Infinity;

            if (opts.stacked) {
                for (var i = 0; i < len; i++) {
                    var tot = 0;

                    for (var j = 0; j < (multi || 1) ; j++) {
                        tot += multi ? values[j][i] : values[i];

                        if (j == multi - 1) {
                            var label = paper.labelise(labels[i], tot, total);

                            L = paper.text(bars[i * (multi || 1) + j].x, y + height - barvgutter / 2, label).attr(txtattr).insertBefore(covers[i * (multi || 1) + j]);

                            var bb = L.getBBox();

                            if (bb.x - 7 < l) {
                                L.remove();
                            } else {
                                this.labels.push(L);
                                l = bb.x + bb.width;
                            }
                        }
                    }
                }
            } else {
                for (var i = 0; i < len; i++) {
                    for (var j = 0; j < (multi || 1) ; j++) {
                        var label = paper.labelise(multi ? labels[j] && labels[j][i] : labels[i], multi ? values[j][i] : values[i], total);

                        L = paper.text(bars[i * (multi || 1) + j].x, isBottom ? y + height - barvgutter / 2 : bars[i * (multi || 1) + j].y - 10, label).attr(txtattr).insertBefore(covers[i * (multi || 1) + j]);

                        var bb = L.getBBox();

                        if (bb.x - 7 < l) {
                            L.remove();
                        } else {
                            this.labels.push(L);
                            l = bb.x + bb.width;
                        }
                    }
                }
            }
            return this;
        };

        chart.hover = function (fin, fout) {
            covers2.hide();
            covers.show();
            covers.mouseover(fin).mouseout(fout);
            return this;
        };

        chart.hoverColumn = function (fin, fout) {
            covers.hide();
            covers2.show();
            fout = fout || function () { };
            covers2.mouseover(fin).mouseout(fout);
            return this;
        };

        chart.click = function (f) {
            covers2.hide();
            covers.show();
            covers.click(f);
            return this;
        };

        chart.each = function (f) {
            if (!Raphael.is(f, "function")) {
                return this;
            }
            for (var i = covers.length; i--;) {
                f.call(covers[i]);
            }
            return this;
        };

        chart.eachColumn = function (f) {
            if (!Raphael.is(f, "function")) {
                return this;
            }
            for (var i = covers2.length; i--;) {
                f.call(covers2[i]);
            }
            return this;
        };

        chart.clickColumn = function (f) {
            covers.hide();
            covers2.show();
            covers2.click(f);
            return this;
        };
        var legend = function (labels, otherslabel, mark, dir) {
            var x = parseInt(dir.split(",")[0]),
                y = parseInt(dir.split(",")[1]),
                h = y + 10;

            labels = labels || [];
            dir = "east";
            dir = (dir && dir.toLowerCase && dir.toLowerCase()) || "east";
            mark = paper[mark && mark.toLowerCase()] || "circle";
            chart.labels = paper.set();

            for (var i = 0; i < labels.length; i++) {
                var clr = colors[i],
                    j = i,
                    txt;
                labels[j] = chartinst.labelise(labels[j], values[i], total);
                chart.labels.push(paper.set());
                chart.labels[i].push(paper[mark](x + 5, h, 5).attr({ fill: clr, stroke: "none" }));
                chart.labels[i].push(txt = paper.text(x + 20, h, labels[j] || values[j]).attr(chartinst.txtattr).attr({ fill: opts.legendcolor || "#000", "text-anchor": "start" }));
                h += txt.getBBox().height * 1.2;
            }
            
            var r = 100;
            var bb = chart.labels.getBBox(),
                tr = {
                    east: [0, -bb.height / 2],
                    west: [-bb.width - 2 * r - 20, -bb.height / 2],
                    north: [-r - bb.width / 2, -r - bb.height - 10],
                    south: [-r - bb.width / 2, r + 10]
                }[dir];

            chart.labels.translate.apply(chart.labels, tr);
            chart.push(chart.labels);
        };

        if (opts.legend) {
            legend(opts.legend, opts.legendothers, opts.legendmark, opts.legendpos);
        }


        chart.push(bars, covers, covers2);
        chart.bars = bars;
        chart.covers = covers;
        return chart;
    };

    //inheritance
    var F = function () { };
    F.prototype = Raphael.g;
    HBarchart.prototype = VBarchart.prototype = new F; //prototype reused by hbarchart

    Raphael.fn.barchart = function (x, y, width, height, values, opts) {
        return new VBarchart(this, x, y, width, height, values, opts);
    };

    /*\
     * Paper.barchart
     [ method ]
     **
     * Creates a horizontal bar chart
     **
     > Parameters
     **
     - x (number) x coordinate of the chart
     - y (number) y coordinate of the chart
     - width (number) width of the chart (respected by all elements in the set)
     - height (number) height of the chart (respected by all elements in the set)
     - values (array) values
     - opts (object) options for the chart
     o {
     o type (string) type of endings of the bar. Default: 'square'. Other options are: 'round', 'sharp', 'soft'.
     o gutter (number)(string) default '20%' (WHAT DOES IT DO?)
     o vgutter (number)
     o colors (array) colors be used repeatedly to plot the bars. If multicolumn bar is used each sequence of bars with use a different color.
     o stacked (boolean) whether or not to tread values as in a stacked bar chart
     o to
     o stretch (boolean)
     o }
     **
     = (object) path element of the popup
     > Usage
     | r.barchart(0, 0, 620, 260, [76, 70, 67, 71, 69], {})
     \*/

    function HBarchart(paper, x, y, width, height, values, opts) {
        opts = opts || {};
        var tValues = values.slice();
        
        for (var i = 0; i < tValues.length; i++) {
            if (tValues[i] < 0) {
                tValues[i] = Math.abs(tValues[i]);
            }
        }

        var chartinst = this,
            type = opts.type || "square",
            gutter = parseFloat(opts.gutter || "20%"),
            chart = paper.set(),
            bars = paper.set(),
            covers = paper.set(),
            covers2 = paper.set(),
            total = Math.max.apply(Math, tValues),
            stacktotal = [],
            multi = 0,
            colors = opts.colors || chartinst.colors,
            len = values.length;
        paper.text(x + width / 2, 30, opts.title ? opts.title : "").attr({ font: "16px Arial", "font-weight": "bold" });
        if (Raphael.is(values[0], "array")) {
            total = [];
            multi = len;
            len = 0;

            for (var i = values.length; i--;) {
                bars.push(paper.set());
                total.push(Math.max.apply(Math, values[i]));
                len = Math.max(len, values[i].length);
            }

            if (opts.stacked) {
                for (var i = len; i--;) {
                    var tot = 0;
                    for (var j = values.length; j--;) {
                        tot += +values[j][i] || 0;
                    }
                    stacktotal.push(tot);
                }
            }

            for (var i = values.length; i--;) {
                if (values[i].length < len) {
                    for (var j = len; j--;) {
                        values[i].push(0);
                    }
                }
            }

            total = Math.max.apply(Math, opts.stacked ? stacktotal : total);
        }

        total = (opts.to) || total;
        total = (total == 0 ? 1 : total);
        var barheight = Math.floor(height / (len * (100 + gutter) + gutter) * 100),
            bargutter = Math.floor(barheight * gutter / 100),
            stack = [],
            Y = y + bargutter,
            X = (width - 1) / total;

        !opts.stacked && (barheight /= multi || 1);
        var labels;
        if (opts.labels) {
            labels = opts.labels.split('|');
        }
        var showValues = true;
        if (typeof opts.showValues != 'undefined') {
            showValues = opts.showValues;
        }
        for (var i = 0; i < len; i++) {
            stack = [];

            for (var j = 0; j < (multi || 1) ; j++) {
                var val = multi ? values[j][i] : values[i],
                    bar = finger(x, Y + barheight / 2, Math.round(val * X), barheight - 1, false, type, null, paper).attr({ stroke: "none", fill: colors[multi ? j : i] });
                var val1 = 0;
                var tri;
                if (opts.markers) {
                    val1 = opts.markers[i];
                    if (val1 > 0) {
                        var flag1;
                        tri = paper.image("res/i/bulGraph.png", x + Math.round(val1 * X) - 4, Y + (barheight / 2) - 5, 8, 10);
                        tri.node.xx = x + Math.round(val1 * X);
                        tri.node.yy = Y + (barheight / 2) - 5;
                        tri.node.vv = val1;
                        tri.node.v = val;
                        if (tri.node) {
                            $(tri.node).hover(function () { flag1 = paper.popup(this.xx, this.yy, "Benchmark: " + parseFloat(this.vv).formatMoney(0, '$') + "\nExpense: " + parseFloat(this.v).formatMoney(0, '$')); }, function () { flag1.hide().remove(); });
                        }
                    }
                }


                if (multi) {
                    bars[j].push(bar);
                } else {
                    bars.push(bar);
                }

                bar.x = x + Math.round(val * X);
                bar.y = Y + barheight / 2;
                bar.w = Math.round(val * X);
                bar.h = barheight;
                bar.value = +val;
                bar.value1 = +val1;
                bar.tri = tri;
                if(!opts.stacked){
                    if (showValues == true) {
                        if (opts.markers) {
                            paper.text(bar.x + 10, bar.y, parseFloat(bar.value || "0").formatMoney(0, '$') + (bar.value1 != 0 ? " (" + (100 * parseFloat(bar.value || "0") / parseFloat(bar.value1 || "0")).toFixed(2) + "%)" : "")).attr({ font: "11px Arial", 'text-anchor': 'start' });
                        }
                        else {
                            paper.text(bar.x + 10, bar.y, parseFloat(bar.value || "0").formatMoney(0, '$')).attr({ font: "11px Arial", 'text-anchor': 'start' });
                        }
                    } else {
                        paper.text(bar.x + 10, bar.y, parseInt(bar.value || "0") + " Day(s)").attr({ font: "11px Arial", 'text-anchor': 'start' });
                    }
                }
                if (!opts.stacked) {
                    Y += barheight;
                } else {
                    stack.push(bar);
                }
                if (bar.tri) { bar.tri.toFront(); }
            }

            if (labels && !opts.stacked) {
                paper.text(x - 10, (multi  ? Y - barheight : Y - barheight/2), labels[i]).attr({ font: '11px Arial', 'text-anchor': 'end' });
            }
            if (opts.stacked) {
                var cvr = paper.rect(x, stack[0].y - stack[0].h / 2, width, barheight).attr(chartinst.shim);

                covers2.push(cvr);
                cvr.bars = paper.set();

                var size = 0;

                for (var s = stack.length; s--;) {
                    stack[s].toFront();
                }
                var stackedValues = 0;
                for (var s = 0, ss = stack.length; s < ss; s++) {
                    var bar = stack[s],
                        cover,
                        val = Math.round((size + bar.value) * X),
                        path = finger(x, bar.y, val, barheight - 1, false, type, 1, paper);

                    cvr.bars.push(bar);
                    size && bar.attr({ path: path });
                    bar.w = val;
                    bar.x = x + val;
                    stackedValues += bar.value;
                    covers.push(cover = paper.rect(x + size * X, bar.y - bar.h / 2, bar.value * X, barheight).attr(chartinst.shim));
                    cover.bar = bar;
                    size += bar.value;
                }

                Y += barheight;
                if (labels) {
                    paper.text(x - 10, Y - barheight/2, labels[i]).attr({ font: '11px Arial', 'text-anchor': 'end' });
                }
                paper.text(bar.x + 10, bar.y, parseFloat(stackedValues || "0").formatMoney(0, '$')).attr({ font: "11px Arial", 'text-anchor': 'start' });
            }

            Y += bargutter;
        }
        if (opts.SeperatorValue) {
            paper.rect(x + Math.round(opts.SeperatorValue * X), 0 + 60, 1, height + 20);
            paper.popup(x + Math.round(opts.SeperatorValue * X) - 10, Y + 20, parseFloat(opts.SeperatorValue).formatMoney(0, '$') + "\nMonthly Benchmark 2014", "left").attr({ font: '11px Arial' });
        }
        //covers2.toFront();
        Y = y + bargutter;

        if (!opts.stacked) {
            for (var i = 0; i < len; i++) {
                for (var j = 0; j < (multi || 1) ; j++) {
                    //var cover = paper.rect(x, Y, width, barheight).attr(chartinst.shim);

                    //covers.push(cover);
                    //cover.bar = multi ? bars[j][i] : bars[i];
                    //cover.value = cover.bar.value;
                    //Y += barheight;
                }

                //Y += bargutter;
            }
        }

        chart.label = function (labels, isRight) {
            labels = labels || [];
            this.labels = paper.set();

            for (var i = 0; i < len; i++) {
                for (var j = 0; j < multi; j++) {
                    var label = paper.labelise(multi ? labels[j] && labels[j][i] : labels[i], multi ? values[j][i] : values[i], total),
                        X = isRight ? bars[i * (multi || 1) + j].x - barheight / 2 + 3 : x + 5,
                        A = isRight ? "end" : "start",
                        L;

                    this.labels.push(L = paper.text(X, bars[i * (multi || 1) + j].y, label).attr(txtattr).attr({ "text-anchor": A }).insertBefore(covers[0]));

                    if (L.getBBox().x < x + 5) {
                        L.attr({ x: x + 5, "text-anchor": "start" });
                    } else {
                        bars[i * (multi || 1) + j].label = L;
                    }
                }
            }

            return this;
        };

        chart.hover = function (fin, fout) {
            covers2.hide();
            covers.show();
            fout = fout || function () { };
            covers.mouseover(fin).mouseout(fout);
            return this;
        };

        chart.hoverColumn = function (fin, fout) {
            covers.hide();
            covers2.show();
            fout = fout || function () { };
            covers2.mouseover(fin).mouseout(fout);
            return this;
        };

        chart.each = function (f) {
            if (!Raphael.is(f, "function")) {
                return this;
            }
            for (var i = covers.length; i--;) {
                f.call(covers[i]);
            }
            return this;
        };

        chart.eachColumn = function (f) {
            if (!Raphael.is(f, "function")) {
                return this;
            }
            for (var i = covers2.length; i--;) {
                f.call(covers2[i]);
            }
            return this;
        };

        chart.click = function (f) {
            covers2.hide();
            covers.show();
            covers.click(f);
            return this;
        };

        chart.clickColumn = function (f) {
            covers.hide();
            covers2.show();
            covers2.click(f);
            return this;
        };
        var legend = function (labels, otherslabel, mark, dir) {
            var x = parseInt(dir.split(",")[0]),
                y = parseInt(dir.split(",")[1]),
                h = y + 10;
            
            labels = labels || [];
            dir = "east";
            dir = (dir && dir.toLowerCase && dir.toLowerCase()) || "east";
            mark = paper[mark && mark.toLowerCase()] || "circle";
            chart.labels = paper.set();

            for (var i = 0; i < labels.length; i++) {
                var clr = colors[i],
                    j = i,
                    txt;
                labels[j] = chartinst.labelise(labels[j], values[i], total);
                chart.labels.push(paper.set());
                chart.labels[i].push(paper[mark](x + 5, h, 5).attr({ fill: clr, stroke: "none" }));
                chart.labels[i].push(txt = paper.text(x + 20, h, labels[j] || values[j]).attr(chartinst.txtattr).attr({ fill: opts.legendcolor || "#000", "text-anchor": "start" }));
                h += txt.getBBox().height * 1.2;
            }
            var r = 100;
            var bb = chart.labels.getBBox(),
                tr = {
                    east: [0, -bb.height / 2],
                    west: [-bb.width - 2 * r - 20, -bb.height / 2],
                    north: [-r - bb.width / 2, -r - bb.height - 10],
                    south: [-r - bb.width / 2, r + 10]
                }[dir];

            chart.labels.translate.apply(chart.labels, tr);
            chart.push(chart.labels);
        };

        if (opts.legend) {
            legend(opts.legend, opts.legendothers, opts.legendmark, opts.legendpos);
        }

        chart.push(bars, covers, covers2);
        chart.bars = bars;
        chart.covers = covers;
        return chart;
    };

    Raphael.fn.hbarchart = function (x, y, width, height, values, opts) {
        return new HBarchart(this, x, y, width, height, values, opts);
    };

    

})();
