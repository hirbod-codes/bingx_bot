const chartElm = document.getElementById('chart');

var netProfit = 0

// closedPositions = closedPositions.slice(closedPositions.length - 73)

var formattedData = closedPositions.map((e, i) => {
    netProfit += e.ProfitWithCommission

    return netProfit
})

grossProfit = longGrossProfit + shortGrossProfit
grossLoss = longGrossLoss + shortGrossLoss

new Chart(chartElm, {
    type: 'bar',
    data: {
        labels: closedPositions.map(e => e.OpenedAt),
        datasets: [{
            label: 'Strategy test results',
            data: formattedData,
            borderWidth: 1,
            barThickness: 5,
            backgroundColor: 'rgb(0, 0, 255, 1)',
            barColor: 'rgb(0, 0, 255, 1)',
        }]
    },
    options: {
        scales: {
            y: {
                beginAtZero: true,
            }
        }
    }
});

document.getElementById('netProfit').innerText = "Net profit: " + netProfit.toString()
document.getElementById('grossProfit').innerText = "Gross profit: " + pnlResults.GrossProfit.toString()
document.getElementById('grossLoss').innerText = "Gross loss: " + pnlResults.GrossLoss.toString()
document.getElementById('highestDrawDown').innerText = "Highest draw down: " + pnlResults.HighestDrawDown.toString()
document.getElementById('highestNetProfit').innerText = "Highest net profit: " + pnlResults.HighestNetProfit.toString()
document.getElementById('positionsCount').innerText = "Positions count: " + closedPositions.length.toString()
document.getElementById('longPositionsCount').innerText = "Long positions count: " + closedPositions.filter(e => e.PositionDirection == "long").length.toString()
document.getElementById('longGrossProfit').innerText = "Long gross profit: " + pnlResults.LongGrossProfit.toString()
document.getElementById('longGrossLoss').innerText = "Long gross loss: " + pnlResults.LongGrossLoss.toString()
document.getElementById('shortPositionsCount').innerText = "Short positions count: " + closedPositions.filter(e => e.PositionDirection == "short").length.toString()
document.getElementById('shortGrossProfit').innerText = "Short gross profit: " + pnlResults.ShortGrossProfit.toString()
document.getElementById('shortGrossLoss').innerText = "Short gross loss: " + pnlResults.ShortGrossLoss.toString()
document.getElementById('shortPositionCount').innerText = "Short position count: " + pnlResults.ShortPositionCount.toString()
document.getElementById('longPositionCount').innerText = "Long position count: " + pnlResults.LongPositionCount.toString()
document.getElementById('openedPositions').innerText = "Opened positions: " + pnlResults.OpenedPositions.toString()
document.getElementById('pendingPositions').innerText = "Pending positions: " + pnlResults.PendingPositions.toString()
document.getElementById('cancelledPositions').innerText = "Cancelled positions: " + pnlResults.CancelledPositions.toString()
document.getElementById('closedPositions').innerText = "Closed positions: " + pnlResults.ClosedPositions.toString()
