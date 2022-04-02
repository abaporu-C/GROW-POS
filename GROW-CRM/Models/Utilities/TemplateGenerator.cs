using GROW_CRM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GROW_CRM.Models.Utilities
{
    public static class TemplateGenerator
    {
        public static string GetHTMLString()
        {
            var sb = new StringBuilder();
            sb.Append(@"
@model GROW_CRM.Models.Order
<html>
<head></head>
<body>
<div class='header'><h2>@Model.ID</h2><div>
<div class='row'>
<fieldset class='form-group border p-4 rounded'>
<legend class='w-auto'>Customer Information'</legend>
<dl class='row'>
<dt class='col-sm-2'>Member</dt>
<dd class='col-sm-10'>@Html.DisplayFor(model => model.Member.FullName)</dd>
<dt class='col-sm-2'>Address</dt>
<dd class='col-sm-10'>@Html.DisplayFor(model => model.Member.Household.FullAddress)</dd>
<dt class='col-sm-2'>Date</dt>
<dd class='col-sm-10'>@Html.DisplayFor(model => model.Date)</dd>
</dl></fieldset></div>
<div class='row'>
<fieldset class='form-group border p-4 rounded'>
<legend class='w-auto'>Purchases</legend>
<dl class='row'>
<dd class='col-sm-10'>
<table class='table table-striped'>
<thead>
<tr>
<th>@Html.DisplayNameFor(modelItem => Model.OrderItems.FirstOrDefault().Item)</th>
<th>@Html.DisplayNameFor(modelItem => Model.OrderItems.FirstOrDefault().Item.Price)</th>
<th>@Html.DisplayNameFor(modelItem => Model.OrderItems.FirstOrDefault().Quantity)</th>
</tr>
</thead>
<tbody>
@foreach(var item in Model.OrderItems)
{
<tr>
<td>@Html.DisplayFor(modelItem => item.Item.Name)</td>
<td>@Html.DisplayFor(modelItem => item.Item.Price)</td>
<td>@Html.DisplayFor(modelItem => item.Quantity)</td>
</tr>
}
</tbody>
</table>
</dd>
</dl>
</fieldset>
</div>
<div class='row'>
<fieldset class='form-group border p-4 rounded'>
<legend class='w-auto'>Payment and Total</legend>
<div class='col-md-3'>
<dl class='col-md-3'>
<dt class='col-sm-2'>@Html.DisplayNameFor(model => model.Total)</dt>
<dd class='col-sm-10'>@Html.DisplayFor(model => model.Total)</dd>
</dl>
<dl class='col-md-3'>
<dt class='col-sm-2'>@Html.DisplayNameFor(model => model.PaymentType)</dt>
<dd class='col-sm-10'>@Html.DisplayFor(model => model.PaymentType.Type)</dd>
</dl>
</fieldset>
</div>
");

            return sb.ToString();
        }
    }
}
