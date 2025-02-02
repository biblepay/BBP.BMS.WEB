

var legend = document.createElement('div');
legend.className = 'sma-legend';
container.appendChild(legend);
legend.style.display = 'block';
legend.style.left = 3 + 'px';
legend.style.top = 3 + 'px';

const toolTipWidth = 80;
const toolTipHeight = 80;
const toolTipMargin = 15;

// Create and style the tooltip html element
const toolTip1 = document.createElement('div');
toolTip1.style = `width: 136px; height: 80px; position: absolute; display: none; padding: 8px; box-sizing: border-box; font-size: 12px; text-align: left; z-index: 1000; top: 12px; left: 12px; pointer-events: none; border: 1px solid; border-radius: 2px;font-family: 'Trebuchet MS', Roboto, Ubuntu, sans-serif; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale;`;
toolTip1.style.background = 'white';
toolTip1.style.color = 'black';
toolTip1.style.borderColor = '#2962FF';
container.appendChild(toolTip1);
function dateToString(date) {
    return `${date.year} - ${date.month} - ${date.day}`;
}
function setTooltip(param)
{
    if (
        param.point === undefined ||
        !param.time ||
        param.point.x < 0 ||
        param.point.x > container.clientWidth ||
        param.point.y < 0 ||
        param.point.y > container.clientHeight
    ) {
        toolTip1.style.display = 'none';
    } else {
        const dateStr = dateToString(param.time);
        toolTip1.style.display = 'block';
        for (j = 0; j < lineseries.length; j++) {
        }
        {
            var i = activeline;

            const price = param.seriesPrices.get(lineseries[i]);
            if (price == undefined) {
                toolTip1.style.display = 'none';
                return;
            }
            if (price) {
                toolTip1.innerHTML = `<div style="color: ${'#2962FF'}">` + legendnames[i].Name + `</div><div style="font-size: 24px; margin: 4px 0px; color: ${'black'}">
			${Math.round(100 * price) / 100}
			</div><div style="color: ${'black'}">
			${dateStr}
			</div>`;

                const coordinate = lineseries[i].priceToCoordinate(price);
                let shiftedCoordinate = param.point.x - 50;
                console.log(coordinate);
                if (coordinate === null) {
                    return;
                }
                shiftedCoordinate = Math.max(
                    0,
                    Math.min(container.clientWidth - toolTipWidth, shiftedCoordinate)
                );
                const coordinateY =
                    coordinate - toolTipHeight - toolTipMargin > 0
                        ? coordinate - toolTipHeight - toolTipMargin
                        : Math.max(
                            0,
                            Math.min(
                                container.clientHeight - toolTipHeight - toolTipMargin,
                                coordinate + toolTipMargin
                            )
                        );
                toolTip1.style.left = shiftedCoordinate + 'px';
                toolTip1.style.top = coordinateY + 'px';
            }
        }
    }
}




chart.subscribeCrosshairMove((param) =>
{
    //areaSeries
    //setTooltip(param);
    var html = '<div id=\"legend1\">' + legendnames[0].Name + '<table class=\"sma-legend-table\">';
    for (i = 0; i < lineseries.length; i++) {
        if (lineseries[i] != null)
        {
            let val = '';

            var price = param.seriesPrices.get(lineseries[i]);
            if (price != undefined) {
                val = (Math.round(price * 100) / 100).toFixed(2);
            }
                
                var sColorDiv = "<tr><td><div style='width:15px;height:15px;background-color:" + legendnames[i].Color + "'></div>";
                var row = sColorDiv + ' <td>' + legendnames[i].Name + '</td><td>' + val + "</td></tr>";
                html += row;
        }
    }
    html += '</table></div>';
    legend.innerHTML = html;
    //'MA10 <span style="color:yellow">' + val + '</span>';

});


/*
 * 
chart.subscribeCrosshairMove(function (param) {

    return;

    if (!param.time || param.point.x < 0 || param.point.x > width || param.point.y < 0 || param.point.y > height) {
        toolTip.style.display = 'none';
        return;
    }


    var dateStr = LightweightCharts.isBusinessDay(param.time)
        ? businessDayToString(param.time)
        : new Date(param.time * 1000).toLocaleDateString();

    toolTip.style.display = 'block';
    var price;
    
    price = param.seriesPrices.get(lineseries[0]);
    if (price == undefined) {
        price = param.seriesPrices.get(lineseries[1]);
        //console.log('1');
        console.log(price);
    }
    // price.close
    toolTip.innerHTML = '<div style="color: rgba(0, 120, 255, 0.9)">⬤ symbol</div>' +
        '<div style="font-size: 24px; margin: 4px 0px; color: #20262E">' + (Math.round(price * 100) / 100).toFixed(2) + '</div>' +
        '<div>' + dateStr + '</div>';

    var left = param.point.x;

    if (left > width - toolTipWidth - toolTipMargin) {
        left = width - toolTipWidth;
    } else if (left < toolTipWidth / 2) {
        left = priceScaleWidth;
    }

    toolTip.style.left = left + 'px';
    toolTip.style.top = container.top + 200 + 'px';
    console.log(toolTip);
});
*/
