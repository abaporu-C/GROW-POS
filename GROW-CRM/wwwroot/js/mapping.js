const allCities = ["Grimsby", "Lincoln", "St. Catharines", "Niagara-on-the-Lake", "Niagara Falls", "Thorold", "Welland", "Pelham", "West Lincoln", "Wainfleet", "Port Colborne", "Fort Erie"]

let mapData = await $.getJSON('/Reports/GetMapJson', function (data) {
    return data;
})

let filledCities = [];

for (let data of mapData) {
    filledCities.push(data.name)
}

for (let city of allCities) {

    const isEmpty = filledCities.some(c => {
        return c === city
    })
    if (!isEmpty) {
        mapData.push({
            "name": city,
            "percentage": 0,
            "total": 0
        })
    }
}


let vegaMap = {
    "$schema": "https://vega.github.io/schema/vega-lite/v5.json",
    "width": 200,
    "height": 200,
                "data": {
                    "url": "../NGMunicipalBoundaries.json",
            "format": { "property": "features" }
    },
    "transform": [{
        "lookup": "properties.Name",
        "from": {
            "data": { "values": JSON.parse(JSON.stringify(mapData)) },
            "key": "name",
            "fields": ["total"]
        },
        "as": "total_members"
    }],
        "projection": { "type": "mercator" },
    "mark": {
        "type": "geoshape",
        "stroke": "#757575",
        "strokeWidth": 1
    },
    "encoding": {
        "color": {
            "field": "total_members",
            "type": "quantitative",
            "legend": {
                "title": "Total Members"
            }
        },
        "tooltip": [
            {
                "field": "properties.Name",
                "type": "nominal",
                "title": "Name"
            },
            {
                "field": "total_members",
                "type": "quantitative",
                "title": "Total Members"
            }
        ]
    }
}

vegaEmbed('#mapping', vegaMap);