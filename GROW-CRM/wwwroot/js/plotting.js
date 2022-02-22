var [gender, age, restriction] = await $.getJSON("/Reports/GetDemoJson", function(data){
    return data;
})

// Assign the specification to a local variable vlSpec.
var vlGender = {
    "data": {
      "values": gender
    },
    "encoding": {
      "theta": {"field": "percentage", "type": "quantitative", "stack": true},
      "color": {"field": "gender", "type": "nominal"}
    },
    "layer": [{
      "mark": {"type": "arc", "outerRadius": 80}
    }, {
      "mark": {"type": "text", "radius": 90},
      "encoding": {
        "text": {"field": "percentageText", "type": "nominal"}
      }
    }]
  };

  var vlAge = {
    "data": {
      "values": age
    },
    "encoding": {
      "theta": {"field": "percentage", "type": "quantitative", "stack": true},
      "color": {"field": "ageRange", "type": "nominal"}
    },
    "layer": [{
      "mark": {"type": "arc", "outerRadius": 80}
    }, {
      "mark": {"type": "text", "radius": 90},
      "encoding": {
        "text": {"field": "percentageText", "type": "nominal"}
      }
    }]
  }

  var vlRestriction = {
    "data": {
      "values": restriction
    },
    "encoding": {
      "theta": {"field": "percentage", "type": "quantitative", "stack": true},
      "color": {"field": "restriction", "type": "nominal"}
    },
    "layer": [{
      "mark": {"type": "arc", "outerRadius": 80}
    }, {
      "mark": {"type": "text", "radius": 90},
      "encoding": {
        "text": {"field": "percentageText", "type": "nominal"}
      }
    }]
  }

  // Embed the visualization in the container with id `vis`
  vegaEmbed('#genderPie', vlGender);
  vegaEmbed('#agePie', vlAge);
  vegaEmbed('#restrictionPie', vlRestriction);