function dumpObject(arr, level) {
    var dumped_text = "";
    if (!level) level = 0;

    var level_padding = "";
    for (var j = 0; j < level + 1; j++) level_padding += "&emsp;&emsp;&emsp;&emsp;";

    if (typeof (arr) == 'object') {
        for (var item in arr) {
            if (["ClosedPositions", "Indicators"].includes(item)) {
                dumped_text += level_padding + "'" + item + "' ... hidden\n";
                continue
            }

            var value = arr[item];

            if (typeof (value) == 'object') {
                dumped_text += level_padding + "'" + item + "' ...\n";
                dumped_text += dumpObject(value, level + 1);
            } else {
                dumped_text += level_padding + "'" + item + "' => \"" + value + "\"\n";
            }
        }
    } else {
        dumped_text = "===>" + arr + "<===(" + typeof (arr) + ")";
    }
    return dumped_text;
}

var buttons = ''

Object.keys(results).forEach(k => {
    buttons += `
    <button type="button" class="btn btn-primary" onclick="selectStrategy('` + k + `')">` + k + `</button>
    `;
})

document.getElementById('buttons').innerHTML = buttons

var chart = null;

var render = (i, strategyName) => {
    if (chart != null)
        chart.destroy();

    chart = null;

    let dump = dumpObject(results[strategyName][i])

    document.getElementById(i + "-body").innerHTML = dump

    let colorTransparency = 'CF'

    let netProfit = 0
    let netProfitWithoutCommission = 0

    let positions = results[strategyName][i].ClosedPositions

    console.log(positions);

    // positions = positions.filter(p => new Date(p.OpenedAt) >= new Date("2024-02-01T00:00:00.0000Z"))

    console.log(positions);

    chart = new Chart(i + '-chart', {
        type: 'bar',
        data: {
            labels: positions.map(e => e.OpenedAt),
            datasets: [
                {
                    label: 'with commission (Cumulative)',
                    pointRadius: 0,
                    data: positions
                        .map((e) => {
                            netProfit += e.ProfitWithCommission

                            return netProfit
                        }),
                    borderWidth: 1,
                    backgroundColor: '#FF0000' + colorTransparency,
                    backgroundColor: '#FF0000' + colorTransparency,
                    borderColor: '#FF0000' + colorTransparency,
                    barColor: '#FF0000' + colorTransparency,
                },
                {
                    label: 'with out commission (Cumulative)',
                    pointRadius: 0,
                    data: positions
                        .map((e) => {
                            netProfitWithoutCommission += e.Profit

                            return netProfitWithoutCommission
                        }),
                    borderWidth: 1,
                    backgroundColor: '#0000FF' + colorTransparency,
                    borderColor: '#0000FF' + colorTransparency,
                    barColor: '#0000FF' + colorTransparency,
                },
                // {
                //     label: '(Commission/SL)(%) (*1)',
                //     pointRadius: 0,
                //     data: positions.map((e) => e.Commission * 1),
                //     borderWidth: 1,
                //     backgroundColor: '#00FF00' + colorTransparency,
                //     borderColor: '#00FF00' + colorTransparency,
                //     barColor: '#00FF00' + colorTransparency,
                // },
                // {
                //     label: 'with commission',
                //     pointRadius: 0,
                //     data: positions.map((e) => e.ProfitWithCommission * 1),
                //     borderWidth: 1,
                //     backgroundColor: '#00FF00' + colorTransparency,
                //     borderColor: '#00FF00' + colorTransparency,
                //     barColor: '#00FF00' + colorTransparency,
                // },
                // {
                //     label: 'with out commission',
                //     pointRadius: 0,
                //     data: positions.map((e) => e.Profit * 1),
                //     borderWidth: 1,
                //     backgroundColor: '#00FF00' + colorTransparency,
                //     borderColor: '#00FF00' + colorTransparency,
                //     barColor: '#00FF00' + colorTransparency
                // },
                // {
                //     label: 'Leverage (x1)',
                //     pointRadius: 0,
                //     data: positions.map((e) => e.Leverage * 1),
                //     borderWidth: 1,
                //     backgroundColor: '#FFFF00' + colorTransparency,
                //     borderColor: '#FFFF00' + colorTransparency,
                //     barColor: '#FFFF00' + colorTransparency,
                // },
                // {
                //     type: 'line',
                //     label: 'Margin (x1)',
                //     pointRadius: 0,
                //     data: positions.map((e) => e.Margin * 1),
                //     borderWidth: 1,
                //     backgroundColor: '#00FFFF' + colorTransparency,
                //     borderColor: '#00FFFF' + colorTransparency,
                //     barColor: '#00FFFF' + colorTransparency,
                // }
            ]
        },
        options: {
            animation: false,
            events: ['click'],
            scales: {
                y: {
                    beginAtZero: true,
                    suggestedMin: results[strategyName][i].PnlResults.HighestDrawDown,
                    suggestedMax: results[strategyName][i].PnlResults.HighestNetProfit
                }
            }
        }
    })
}

var selectStrategy = (strategyName) => {
    document.getElementById("accordion").innerHTML = ``

    results[strategyName].forEach((result, i) => {
        document.getElementById("accordion").innerHTML += `
                        <div class="accordion-item">
                            <h2 class="accordion-header" id="heading` + i + `">
                                <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#collapse` + i + `" id="` + i + `button" onclick="render('` + i + `', '` + strategyName + `')">
                                ` + i + `
                                </button>
                            </h2>
                            <div id="collapse` + i + `" class="accordion-collapse collapse" data-bs-parent="#accordion">
                                <div class="accordion-body" style="">
                                    <div id="` + i + `" style="margin: 10px;border: 1px solid red;">
                                        <canvas id="` + i + `-chart"></canvas>
                                    </div>
        
                                    <pre style="max-height: 40em;" id="` + i + `-body">
                                    </pre>
                                </div>
                            </div>
                        </div>
                    `;
    })

    document.getElementById("title").innerHTML = strategyName
}

selectStrategy(Object.keys(results)[0])