const chartElm = document.getElementById('chart');

var highestNetProfit = 0
var highestDrawDown = 0
var draDown = 0
var netProfit = 0
var longGrossProfit = 0;
var shortGrossProfit = 0;
var grossProfit = 0;
var longGrossLoss = 0;
var shortGrossLoss = 0;
var grossLoss = 0;

data = data.slice(0, 100)

var formattedData = data.map((e, i) => {
    if (e.PositionStatus != "closed")
        return netProfit

    netProfit += e.ProfitWithCommission

    if (netProfit > highestNetProfit)
        highestNetProfit = netProfit

    draDown = highestNetProfit - netProfit

    if (draDown > highestNetProfit)
        highestDrawDown = draDown

    if (e.PositionDirection == "long")
        if (e.ProfitWithCommission > 0)
            longGrossProfit += e.ProfitWithCommission
        else
            longGrossLoss += e.ProfitWithCommission
    else
        if (e.ProfitWithCommission > 0)
            shortGrossProfit += e.ProfitWithCommission
        else
            shortGrossLoss += e.ProfitWithCommission

    return netProfit
})

grossProfit = longGrossProfit + shortGrossProfit
grossLoss = longGrossLoss + shortGrossLoss

new Chart(chartElm, {
    type: 'bar',
    data: {
        labels: data.map(e => e.Id),
        datasets: [{
            label: 'Strategy test results',
            data: formattedData,
            borderWidth: 1
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
document.getElementById('grossProfit').innerText = "Gross profit: " + grossProfit.toString()
document.getElementById('grossLoss').innerText = "Gross loss: " + grossLoss.toString()
document.getElementById('highestNetProfit').innerText = "Highest net profit: " + highestNetProfit.toString()
document.getElementById('highestDrawDown').innerText = "Highest draw down: " + highestDrawDown.toString()
document.getElementById('longGrossProfit').innerText = "Long gross profit: " + longGrossProfit.toString()
document.getElementById('shortGrossProfit').innerText = "Short gross profit: " + shortGrossProfit.toString()
document.getElementById('longGrossLoss').innerText = "Long gross loss: " + longGrossLoss.toString()
document.getElementById('shortGrossLoss').innerText = "Short gross loss: " + shortGrossLoss.toString()
