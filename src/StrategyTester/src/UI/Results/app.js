function dumpObject(arr, level) {
    var dumped_text = "";
    if (!level) level = 0;

    var level_padding = "";
    for (var j = 0; j < level + 1; j++) level_padding += "&emsp;&emsp;&emsp;&emsp;";

    if (typeof (arr) == 'object') {
        for (var item in arr) {
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

    var strategiesClosedPositions = []

    let t = results[strategyName][i].ClosedPositions

    strategiesClosedPositions.push(results[strategyName][i].ClosedPositions.filter(e => e != null))

    results[strategyName][i].ClosedPositions = null
    let dump = dumpObject(results[strategyName][i])
    results[strategyName][i].ClosedPositions = t

    document.getElementById(i + "-body").innerHTML = dump

    let netProfit = 0
    let netProfitWithoutCommission = 0

    chart = new Chart(i + '-chart', {
        type: 'bar',
        data: {
            labels: results[strategyName][i].ClosedPositions.map(e => e.OpenedAt),
            datasets: [
                {
                    label: 'Strategy test results',
                    data: results[strategyName][i].ClosedPositions.map((e) => {
                        netProfit += e.ProfitWithCommission

                        return netProfit
                    }),
                    borderWidth: 1,
                    backgroundColor: '#FF0000B3',
                    borderColor: '#FF0000B3',
                    barColor: '#FF0000B3',
                },
                {
                    label: 'Strategy test results (with out commission)',
                    data: results[strategyName][i].ClosedPositions.map((e) => {
                        netProfitWithoutCommission += e.Profit

                        return netProfitWithoutCommission
                    }),
                    borderWidth: 1,
                    backgroundColor: '#0000FFB3',
                    borderColor: '#0000FFB3',
                    barColor: '#0000FFB3',
                }
            ]
        },
        options: {
            animation: false,
            events: ['click'],
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
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
                                <div class="accordion-body">
                                    <div id="` + i + `" style="margin: 10px;border: 1px solid red;height:500px">
                                        <canvas id="` + i + `-chart"></canvas>
                                    </div>
        
                                    <pre style="max-height: 40em;" id="` + i + `-body">
                                    </pre>
                                </div>
                            </div>
                        </div>
                    `;
    })
}

selectStrategy(Object.keys(results)[0])