
function createSimpleSwitcher(items, activeItem, activeItemChangedCallback) {
        var switcherElement = document.createElement('div');
        switcherElement.classList.add('switcher');
        var intervalElements = items.map(function (item) {
            var itemEl = document.createElement('button');
            itemEl.innerText = item;
            itemEl.classList.add('switcher-item');
            itemEl.classList.toggle('switcher-active-item', item === activeItem);
            itemEl.addEventListener('click', function () {
                onItemClicked(item);
            });
            switcherElement.appendChild(itemEl);
            return itemEl;
        });

        function onItemClicked(item) {
            if (item === activeItem) { 
                //return; 
            }

            intervalElements.forEach(function (element, index) {
                //element.classList.toggle('switcher-active-item', items[index] === item);
            });

            activeItem = item;

            activeItemChangedCallback(item);
        }
        return switcherElement;
    }



    var darkTheme = {
        chart: {
            layout: {
                backgroundColor: '#2B2B43',
                lineColor: '#2B2B43',
                textColor: '#D9D9D9',
            },
            rightPriceScale: {
                visible: false,
            },
            leftPriceScale: {
                visible: false,
            },
            rightPriceScale: {
                scaleMargins: {
                    top: 0.1,
                    bottom: 0.1,
                },
                borderColor: 'rgba(197, 203, 206, 0.4)',
            },
            watermark: {
                color: 'rgba(0, 0, 0, 0)',
            },
            crosshair: {
                color: '#758696',
            },
            grid: {
                vertLines: {
                    color: '#2B2B43',
                },
                horzLines: {
                    color: '#363C4E',
                },
            },
        },
        series: {
            topColor: 'rgba(32, 226, 47, 0.56)',
            bottomColor: 'rgba(32, 226, 47, 0.04)',
            lineColor: 'rgba(32, 226, 47, 1)',
        },
    };

    const lightTheme = {
        chart: {
            layout: {
                backgroundColor: '#FFFFFF',
                lineColor: '#2B2B43',
                textColor: '#191919',
            },
            watermark: {
                color: 'rgba(0, 0, 0, 0)',
            },
            grid: {
                vertLines: {
                    visible: false,
                },
                horzLines: {
                    color: '#f0f3fa',
                },
            },
        },
        series: {
            topColor: 'rgba(33, 150, 243, 0.56)',
            bottomColor: 'rgba(33, 150, 243, 0.04)',
            lineColor: 'rgba(33, 150, 243, 1)',
        },
    };


    function businessDayToString(businessDay) {
        return new Date(Date.UTC(businessDay.year, businessDay.month - 1, businessDay.day, 0, 0, 0)).toLocaleDateString();
    }


var priceScaleWidth = 50;

var width = 750;
var height = 400;
var container = document.getElementById('chartid0');

var switcherElement = createSimpleSwitcher(['Dark', 'Light'], 'Dark', syncToTheme);

    var toolTip = document.createElement('div');
    toolTip.className = 'floating-tooltip-2';
    container.appendChild(toolTip);
    //container.appendChild(switcherElement);

    var themesData = {
        Dark: darkTheme,
        Light: lightTheme,
    };


    function syncToTheme(theme) {
        chart.applyOptions(themesData[theme].chart);
        //areaSeries.applyOptions(themesData[theme].series);
    }

