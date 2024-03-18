candles = candles.slice(203, 263)

closedPositions = results.CandlesOpenClose[0].ClosedPositions.filter(o => new Date(o.OpenedAt) > new Date(candles[0].Date) && new Date(o.OpenedAt) < new Date(candles[candles.length - 1].Date))

// var startDate = new Date("2024-01-14T22:00:00")
// var endDate = new Date("2024-01-15T08:47:00")
var startDate = new Date(candles[0].Date)
var endDate = new Date(candles[candles.length - 1].Date)
var start = startDate.valueOf();
var end = endDate.valueOf();
var dateRange = [startDate.toISOString(), endDate.toISOString()]
var timestampRange = [start, end]

candles = candles.filter(c => Date.parse(c.Date) <= end && Date.parse(c.Date) >= start)

var data = [
    {
        x: candles.map(c => c.Date),
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
    {
        x: closedPositions.map(p => p.OpenedAt),
        y: closedPositions.map(p => p.TPPrice),

        mode: 'markers',
        type: 'scatter',
        name: 'TPPrice',
        text: closedPositions.map(p => p.TPPrice),
        marker: { size: 4 }
    },
    {
        x: closedPositions.map(p => p.OpenedAt),
        y: closedPositions.map(p => p.OpenedPrice),

        mode: 'markers',
        type: 'scatter',
        name: 'OpenedPrice',
        text: closedPositions.map(p => p.OpenedPrice),
        marker: { size: 4 }
    },
    {
        x: closedPositions.map(p => p.OpenedAt),
        y: closedPositions.map(p => p.SLPrice),

        mode: 'markers',
        type: 'scatter',
        name: 'SLPrice',
        text: closedPositions.map(p => p.SLPrice),
        marker: { size: 4 }
    },
    {
        x: closedPositions.map(p => p.OpenedAt),
        y: closedPositions.map(p => p.ClosedPrice),

        mode: 'lines+markers',
        type: 'scatter',
        name: 'ClosedPrice',
        text: closedPositions.map(p => p.ClosedPrice),
        marker: { size: 4 }
    }
];

var layout = {
    dragmode: 'zoom',
    margin: {
        r: 10,
        t: 25,
        b: 40,
        l: 60
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
        range: timestampRange,
        title: 'Price',
        type: 'linear'
    }
};

Plotly.newPlot('chart', data, layout);
