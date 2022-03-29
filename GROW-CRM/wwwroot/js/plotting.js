//TODO: create overload of GetDemoJson function to accept an argument for type of data, to get a specific set of data instead of all of them.

var [gender, age, restriction] = await $.getJSON("/Reports/GetDemoJson", function (data) {
    return data;
})

am4core.ready(function () {

    // Themes begin
    am4core.useTheme(am4themes_animated);
    // Themes end


    //Gender Chart
    var genderChart = am4core.create("genderPie", am4charts.PieChart);
    genderChart.hiddenState.properties.opacity = 0; // this creates initial fade-in

    genderChart.data = gender;
    genderChart.radius = am4core.percent(70);
    genderChart.innerRadius = am4core.percent(40);
    genderChart.startAngle = 180;
    genderChart.endAngle = 360;

    var series = genderChart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "total";
    series.dataFields.category = "gender";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.draggable = true;
    series.slices.template.inert = true;
    series.alignLabels = false;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    genderChart.legend = new am4charts.Legend();

    //Age Chart

    var ageChart = am4core.create("agePie", am4charts.PieChart);
    ageChart.hiddenState.properties.opacity = 0; // this creates initial fade-in

    ageChart.data = age;
    ageChart.radius = am4core.percent(70);
    ageChart.innerRadius = am4core.percent(40);
    ageChart.startAngle = 180;
    ageChart.endAngle = 360;

    var series = ageChart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "total";
    series.dataFields.category = "ageRange";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.draggable = true;
    series.slices.template.inert = true;
    series.alignLabels = false;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    ageChart.legend = new am4charts.Legend();

    //Restriction Chart

    var restrictionChart = am4core.create("restrictionPie", am4charts.PieChart);
    restrictionChart.hiddenState.properties.opacity = 0; // this creates initial fade-in

    restrictionChart.data = restriction;
    restrictionChart.radius = am4core.percent(70);
    restrictionChart.innerRadius = am4core.percent(40);
    restrictionChart.startAngle = 180;
    restrictionChart.endAngle = 360;

    var series = restrictionChart.series.push(new am4charts.PieSeries());
    series.dataFields.value = "total";
    series.dataFields.category = "restriction";

    series.slices.template.cornerRadius = 10;
    series.slices.template.innerCornerRadius = 7;
    series.slices.template.draggable = true;
    series.slices.template.inert = true;
    series.alignLabels = false;

    series.hiddenState.properties.startAngle = 90;
    series.hiddenState.properties.endAngle = 90;

    restrictionChart.legend = new am4charts.Legend();

    console.log(am4core)

}); // end am4core.ready()

