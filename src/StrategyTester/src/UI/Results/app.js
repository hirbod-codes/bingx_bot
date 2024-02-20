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

Object.keys(results).forEach(k => {
    document.getElementById("accordion").innerHTML += `
        <div class="accordion-item">
            <h2 class="accordion-header" id="heading` + k + `">
                <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#collapse` + k + `">
                ` + k + `
                </button>
            </h2>
            <div id="collapse` + k + `" class="accordion-collapse collapse show" data-bs-parent="#accordion">
                <div class="accordion-body">
                    <div class="accordion" id="` + k + `accordion">
                    </div>
                </div>
            </div>
        </div>
    `

    results[k].forEach((result, i) => {
        var closedPositions = result.ClosedPositions

        delete result.ClosedPositions

        var dump = dumpObject(result)

        document.getElementById(k + "accordion").innerHTML += `
                <div class="accordion-item">
                    <h2 class="accordion-header" id="heading` + k + i + `">
                        <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#collapse` + k + i + `">
                        ` + i + `
                        </button>
                    </h2>
                    <div id="collapse` + k + i + `" class="accordion-collapse collapse show" data-bs-parent="#` + k + `accordion">
                        <div class="accordion-body">
                            <pre id="` + k + i + `-body">
                            </pre>
        
                            <div style="margin: 10px;border: 1px solid red;">
                                <canvas id="` + k + i + `-chart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>
            `;

        document.getElementById(k + i + "-body").innerHTML = dump

        const chartElm = document.getElementById(k + i + '-chart')

        var netProfit = 0
        var closedPositions = closedPositions.filter(e => e != null)
        var formattedData = closedPositions.map((e, i) => {
            netProfit += e.ProfitWithCommission

            return netProfit
        })

        grossProfit = result.PnlResults.LongGrossProfit + result.PnlResults.ShortGrossProfit
        grossLoss = result.PnlResults.LongGrossLoss + result.PnlResults.ShortGrossLoss

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
        })
    })
})
