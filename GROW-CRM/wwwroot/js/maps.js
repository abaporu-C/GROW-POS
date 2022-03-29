let mapData = await $.getJSON('/Reports/GetMapJson', function (data) {
    return data;
})

let mapDataTempered = mapData.map(data => {
    return { id: data.name, value: data.total }
});

let gJson = await $.getJSON('../NGMunicipalBoundaries.json', function (data) {
    return data;
})

let gJsonTempered = [];

for (let city of gJson.features) {    
    let newData = mapData.find(d => d.name === city.properties.Name)
    let newCity = {
        ...city, properties: {
            ...city.properties,
            id: city.properties.Name,
            value: newData.total
        }
    }
    gJsonTempered.push(newCity);
}

let newgJson = {
    ...gJson,
    features: gJsonTempered
}

am4core.useTheme(am4themes_animated);

// Create map instance
var chart = am4core.create("mapping", am4maps.MapChart);

// Set map definition
chart.geodata = newgJson;

// Set projection
chart.projection = new am4maps.projections.Mercator();

// Create map polygon series
var polygonSeries = chart.series.push(new am4maps.MapPolygonSeries());

//Set min/max fill color for each area
polygonSeries.heatRules.push({
    property: "fill",
    target: polygonSeries.mapPolygons.template,
    min: chart.colors.getIndex(1).brighten(1),
    max: chart.colors.getIndex(1).brighten(-0.3),
    logarithmic: true
});

// Make map load polygon data (state shapes and names) from GeoJSON
polygonSeries.useGeodata = true;

// Set heatmap values for each state
polygonSeries.data = mapDataTempered;

// Set up heat legend
let heatLegend = chart.createChild(am4maps.HeatLegend);
heatLegend.series = polygonSeries;
heatLegend.align = "right";
heatLegend.valign = "bottom";
heatLegend.height = am4core.percent(80);
heatLegend.orientation = "vertical";
heatLegend.valign = "middle";
heatLegend.marginRight = am4core.percent(4);
heatLegend.valueAxis.renderer.opposite = true;
heatLegend.valueAxis.renderer.dx = - 25;
heatLegend.valueAxis.strictMinMax = true;
heatLegend.valueAxis.fontSize = 9;
heatLegend.valueAxis.logarithmic = true;

// Configure series tooltip
var polygonTemplate = polygonSeries.mapPolygons.template;
polygonTemplate.tooltipText = "{Name}: {value}";
polygonTemplate.nonScalingStroke = true;
polygonTemplate.strokeWidth = 0.5;

// Create hover state and set alternative fill color
var hs = polygonTemplate.states.create("hover");
hs.properties.fill = am4core.color("#3c5bdc");


// heat legend behavior
polygonSeries.mapPolygons.template.events.on("over", function (event) {
    handleHover(event.target);
})

polygonSeries.mapPolygons.template.events.on("hit", function (event) {
    handleHover(event.target);
})

function handleHover(column) {
    if (!isNaN(column.dataItem.value)) {
        heatLegend.valueAxis.showTooltipAt(column.dataItem.value)
    }
    else {
        heatLegend.valueAxis.hideTooltip();
    }
}

polygonSeries.mapPolygons.template.events.on("out", function (event) {
    heatLegend.valueAxis.hideTooltip();
})