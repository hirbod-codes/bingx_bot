Date.prototype.addMilliseconds = function (ms) {
    this.setTime(this.getTime() + (ms));
    return this;
}

console.log('candles.length', candles.length);

// var startDate = new Date("2021-06-27T00:00:00.000Z");
// var endDate = new Date("2024-01-15T08:47:00")
var startDate = new Date(candles[0].Date)
var endDate = new Date(candles[candles.length - 1].Date)

candles = candles.filter(c => new Date(c.Date) >= startDate && new Date(c.Date) < endDate)

// candles = candles.slice(candles.length - 101)
candles = candles.slice(200, 600)

closedPositions = results.CandlesOpenClose[0].ClosedPositions.filter(o => new Date(o.OpenedAt) >= new Date(candles[0].Date) && new Date(o.OpenedAt) < new Date(candles[candles.length - 1].Date))

var start = startDate.valueOf();
var end = endDate.valueOf();
var dateRange = [startDate.toISOString(), endDate.toISOString()]

candles = candles.filter(c => Date.parse(c.Date) <= end && Date.parse(c.Date) >= start)

var indicators = results.CandlesOpenClose[0].PnlResults.Indicators

var overlayData = []
var data = []

Object.keys(indicators).forEach(k => {
    let indicator = indicators[k].filter(o => new Date(o.Date) >= new Date(candles[0].Date) && new Date(o.Date) < new Date(candles[candles.length - 1].Date))
    let x = indicator.map(i => new Date(i.Date))
    let y = []
    switch (k) {
        case "_atr":
            y = indicator.map(i => i.Atr)
            break;
        case "_stochastic":
            y = indicator.map(i => i.K)
            break;
        case "_deltaWma":
            y = indicator.map(i => i.Wma)
            break;
        case "_rsi":
            y = indicator.map(i => i.rsi)
            break;

        default:
            break;
    }

    if (k == "_superTrend") {
        data.push({
            x,
            y: indicator.map(i => i.LowerBand),
            mode: 'lines',
            type: 'scatter',
            name: 'LowerBand',
            text: y
        })
        data.push({
            x,
            y: indicator.map(i => i.UpperBand),
            mode: 'lines',
            type: 'scatter',
            name: 'UpperBand',
            text: y
        })
    }
    else
        overlayData.push({
            x,
            y,
            mode: 'lines',
            type: 'scatter',
            name: k,
            text: y
        })
})

data = data.concat([
    {
        x: candles.map(c => new Date(c.Date)),
        close: candles.map(c => c.Close),
        high: candles.map(c => c.High),
        low: candles.map(c => c.Low),
        open: candles.map(c => c.Open),

        decreasing: { line: { color: '#7F7F7F' } },
        increasing: { line: { color: '#17BECF' } },
        line: { color: 'rgba(31,119,180,1)' },
        type: 'candlestick',
        xaxis: 'x',
        yaxis: 'y'
    },
    // {
    //     x: closedPositions.map(p => new Date(p.OpenedAt)),
    //     y: closedPositions.map(p => p.TPPrice),

    //     mode: 'markers',
    //     type: 'scatter',
    //     name: 'TPPrice',
    //     text: closedPositions.map(p => p.TPPrice),
    //     marker: {
    //         size: 4,
    //         color: 12
    //     }
    // },
    // {
    //     x: closedPositions.map(p => new Date(p.OpenedAt)),
    //     y: closedPositions.map(p => p.OpenedPrice),

    //     mode: 'markers',
    //     type: 'scatter',
    //     name: 'OpenedPrice',
    //     text: closedPositions.map(p => p.OpenedPrice),
    //     marker: {
    //         size: 4,
    //         color: 0
    //     }
    // },
    // {
    //     x: closedPositions.map(p => new Date(p.OpenedAt)),
    //     y: closedPositions.map(p => p.SLPrice),

    //     mode: 'markers',
    //     type: 'scatter',
    //     name: 'SLPrice',
    //     text: closedPositions.map(p => p.SLPrice),
    //     marker: {
    //         size: 4,
    //         color: 12
    //     }
    // },
    {
        x: closedPositions.map(p => new Date(p.OpenedAt)),
        y: closedPositions.map(p => p.ClosedPrice),

        mode: 'lines+markers',
        type: 'scatter',
        name: 'ClosedPrice',
        text: closedPositions.map(p => p.ClosedPrice),
        marker: {
            size: 4,
            color: 12
        }
    },
    // {
    //     x: candles.filter(c => Math.abs(c.Open - c.Close) >= (Math.abs(c.Open - c.Close) / 4.0)).map(c => c.Date),
    //     y: candles.filter(c => Math.abs(c.Open - c.Close) >= (Math.abs(c.Open - c.Close) / 4.0)).map(c => Math.max(c.Open, c.Close) + Math.abs(c.Open - c.Close) * 10),

    //     mode: 'markers',
    //     type: 'scatter',
    //     name: 'Is Candle Valid',
    //     text: candles.filter(c => Math.abs(c.Open - c.Close) >= (Math.abs(c.Open - c.Close) / 4.0)).map(c => Math.max(c.Open, c.Close) + Math.abs(c.Open - c.Close) * 10),
    //     marker: {
    //         size: 8,
    //         color: 12
    //     }
    // }
]);

Plotly.newPlot('overlay-chart', overlayData, {
    dragmode: 'pan',
    margin: {
        r: 10,
        t: 25,
        b: 40,
        l: 160
    },
    showlegend: false,
    xaxis: {
        autorange: true,
        domain: [0, 1],
        range: dateRange,
        rangeslider: { range: dateRange },
        title: 'Date',
        type: 'date'
    },
    yaxis: {
        autorange: true,
        domain: [0, 1],
        title: 'value',
        type: 'linear'
    }
}, { responsive: true });

Plotly.newPlot('chart', data, {
    shapes: closedPositions.map((p, i) => [{
        type: 'rect',
        xref: 'x',
        yref: 'y',
        x0: p.OpenedAt,
        x1: p.ClosedAt,
        y1: Math.max(p.SLPrice, p.TPPrice),
        y0: p.OpenedPrice,
        fillcolor: '#0a0',
        opacity: 0.3,
        line: {
            width: 0
        }
    },
    {
        type: 'rect',
        xref: 'x',
        yref: 'y',
        x0: p.OpenedAt,
        x1: p.ClosedAt,
        y1: p.OpenedPrice,
        y0: Math.min(p.SLPrice, p.TPPrice),
        fillcolor: '#a00',
        opacity: 0.2,
        line: {
            width: 0
        }
    }]).flat(),
    dragmode: 'pan',
    margin: {
        r: 10,
        t: 25,
        b: 40,
        l: 160
    },
    showlegend: false,
    xaxis: {
        autorange: true,
        domain: [0, 1],
        range: dateRange,
        rangeslider: { range: dateRange },
        title: 'Date',
        type: 'date'
    },
    yaxis: {
        // autorange: true,
        domain: [0, 1],
        range: [8600, 10000],
        title: 'Price',
        type: 'linear'
    }
}, { responsive: true });
