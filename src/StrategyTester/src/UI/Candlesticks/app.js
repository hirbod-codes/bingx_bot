Date.prototype.addMilliseconds = function (ms) {
    this.setTime(this.getTime() + (ms));
    return this;
}

result = results.EmaRsi[0]

// candles = candles.slice(candles.length - 101)
candles = candles.slice(200, 600)

console.log('candles.length', candles.length)

// var startDate = new Date("2021-06-27T00:00:00.000Z");
// var endDate = new Date("2024-01-15T08:47:00")
var startDate = new Date(candles[0].Date)
var endDate = new Date(candles[candles.length - 1].Date)

candles = candles.filter(c => new Date(c.Date) >= startDate && new Date(c.Date) <= endDate)

closedPositions = result.ClosedPositions.filter(o => new Date(o.OpenedAt) >= new Date(candles[0].Date) && new Date(o.OpenedAt) <= new Date(candles[candles.length - 1].Date))

var dateRange = [new Date(candles[0].Date).toISOString(), new Date(candles[candles.length - 1].Date).toISOString()]

var indicators = result.PnlResults.Indicators

var overlayData = []
var data = []

Object.keys(indicators).forEach(k => {
    let indicator = indicators[k].filter(o => new Date(o.Date) >= new Date(candles[0].Date) && new Date(o.Date) < new Date(candles[candles.length - 1].Date))
    let x = indicator.map(i => new Date(i.Date))
    let y = []
    if (k.includes("_atr"))
        y = indicator.map(i => i.Atr)
    else if (k.includes("_stochastic"))
        y = indicator.map(i => i.K)
    else if (k.includes("_deltaWma"))
        y = indicator.map(i => i.Wma)
    else if (k.includes("_rsi"))
        y = indicator.map(i => i.Rsi)
    else if (k.includes("_ema"))
        y = indicator.map(i => i.Ema)

    if (k.includes("_superTrend")) {
        data.push({
            x,
            y: indicator.map(i => i.LowerBand),
            mode: 'lines',
            name: k.replace('_superTrend', '') + 'UpperBand',
            type: 'scatter',
            text: y
        })
        data.push({
            x,
            y: indicator.map(i => i.UpperBand),
            mode: 'lines',
            type: 'scatter',
            name: k.replace('_superTrend', '') + 'UpperBand',
            text: y
        })
    }
    else if (k.includes("_wma")) {
        data.push({
            x,
            y,
            mode: 'lines',
            type: 'scatter',
            name: k,
            text: y
        })
    }
    else if (k.includes("_ema")) {
        data.push({
            x,
            y,
            mode: 'lines',
            type: 'scatter',
            name: k,
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

        increasing: { fillcolor: 'rgba(255, 255, 255,0)', line: { width: 1, color: '#0f0' } },
        decreasing: { fillcolor: 'rgba(255, 255, 255,0)', line: { width: 1, color: '#f00' } },
        line: { color: 'rgba(31,119,180,1)', width: 1 },
        type: 'candlestick',
        name: 'BTC-USDT.PS',
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

Plotly.newPlot(
    'overlay-chart',
    overlayData,
    {
        autocolorscale: true,
        paper_bgcolor: '#2d334f',
        plot_bgcolor: '#2d334f',
        font: {
            color: '#aaa'
        },
        dragmode: 'pan',
        margin: {
            r: 10,
            t: 25,
            b: 40,
            l: 160
        },
        showlegend: true,
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
    },
    {
        responsive: true,
        scrollZoom: true
    }
);

Plotly.newPlot(
    'chart',
    data,
    {
        shapes: closedPositions.map((p, i) => {
            if (!p.TPPrice)
                p.TPPrice = p.OpenedPrice

            return [
                {
                    type: 'rect',
                    xref: 'x',
                    yref: 'y',
                    x0: new Date(p.CreatedAt).addMilliseconds(-1 * result.BrokerOptions.TimeFrame * 1000),
                    x1: new Date(p.OpenedAt).addMilliseconds(-1 * result.BrokerOptions.TimeFrame * 1000),
                    y1: Math.max(p.SLPrice, p.TPPrice),
                    y0: Math.min(p.SLPrice, p.TPPrice),
                    fillcolor: '#aaa',
                    opacity: 0.3,
                    line: {
                        width: 0
                    },
                    label: {
                        text: 'pending'
                    }
                },
                {
                    type: 'rect',
                    xref: 'x',
                    yref: 'y',
                    x0: new Date(p.OpenedAt).addMilliseconds(-1 * result.BrokerOptions.TimeFrame * 1000),
                    x1: new Date(p.ClosedAt).addMilliseconds(-1 * result.BrokerOptions.TimeFrame * 1000),
                    y1: Math.max(p.SLPrice, p.TPPrice),
                    y0: p.OpenedPrice,
                    fillcolor: p.PositionDirection.toLowerCase() == "long" ? '#0f0' : '#f00',
                    opacity: 0.3,
                    line: {
                        width: 0
                    },
                    label: {
                        text: 'top',
                        textposition: 'top center'
                    }
                },
                {
                    type: 'rect',
                    xref: 'x',
                    yref: 'y',
                    x0: new Date(p.OpenedAt).addMilliseconds(-1 * result.BrokerOptions.TimeFrame * 1000),
                    x1: new Date(p.ClosedAt).addMilliseconds(-1 * result.BrokerOptions.TimeFrame * 1000),
                    y1: p.OpenedPrice,
                    y0: Math.min(p.SLPrice, p.TPPrice),
                    fillcolor: p.PositionDirection.toLowerCase() == "long" ? '#f00' : '#0f0',
                    opacity: 0.3,
                    line: {
                        width: 0
                    },
                    label: {
                        text: 'bottom',
                        textposition: 'bottom center'
                    }
                }
            ]
        }).flat(),
        autocolorscale: true,
        paper_bgcolor: '#2d334f',
        plot_bgcolor: '#2d334f',
        font: {
            color: '#aaa'
        },
        dragmode: 'pan',
        margin: {
            r: 10,
            t: 25,
            b: 40,
            l: 160
        },
        showlegend: true,
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
            // range: [30000, 40000],
            title: 'Price',
            type: 'linear'
        }
    },
    {
        responsive: true,
        scrollZoom: true
    }
);
