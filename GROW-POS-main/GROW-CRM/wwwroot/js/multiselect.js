let DDLforChosenIllness = document.getElementById("selectedIllnessOptions");
let DDLforAvailIllness = document.getElementById("availIllnessOptions");
let DDLforChosenConcern = document.getElementById("selectedConcernOptions");
let DDLforAvailConcern = document.getElementById("availConcernOptions");

/*function to switch list items from one ddl to another
use the sender param for the DDL from which the user is multi-selecting
use the receiver param for the DDL that gets the options*/
function switchIllnessOptions(event, senderDDL, receiverDDL) {
    //find all selected option tags - selectedOptions becomes a nodelist 
    let senderID = senderDDL.id;
    let selectedOptions = document.querySelectorAll(`#${senderID} option:checked`);
    event.preventDefault();

    if (selectedOptions.length === 0) {
        alert("Nothing to move.");
    }
    else {
        selectedOptions.forEach(function (o, idx) {
            senderDDL.remove(o.index);
            receiverDDL.appendChild(o);
        });
    }
}
//create closures so that we can access the event & the 2 parameters
let addIllnessOptions = (event) => switchIllnessOptions(event, DDLforAvailIllness, DDLforChosenIllness);
let removeIllnessOptions = (event) => switchIllnessOptions(event, DDLforChosenIllness, DDLforAvailIllness);
//assign the closures as the event handlers for each button
document.getElementById("btnIllnessLeft").addEventListener("click", addIllnessOptions);
document.getElementById("btnIllnessRight").addEventListener("click", removeIllnessOptions);

function switchConcernOptions(event, senderDDL, receiverDDL) {
    //find all selected option tags - selectedOptions becomes a nodelist 
    let senderID = senderDDL.id;
    let selectedOptions = document.querySelectorAll(`#${senderID} option:checked`);
    event.preventDefault();

    if (selectedOptions.length === 0) {
        alert("Nothing to move.");
    }
    else {
        selectedOptions.forEach(function (o, idx) {
            senderDDL.remove(o.index);
            receiverDDL.appendChild(o);
        });
    }
}

//create closures so that we can access the event & the 2 parameters
let addConcernOptions = (event) => switchConcernOptions(event, DDLforAvailConcern, DDLforChosenConcern);
let removeConcernOptions = (event) => switchConcernOptions(event, DDLforChosenConcern, DDLforAvailConcern);
//assign the closures as the event handlers for each button
document.getElementById("btnConcernLeft").addEventListener("click", addConcernOptions);
document.getElementById("btnConcernRight").addEventListener("click", removeConcernOptions);

document.getElementById("btnSubmit").addEventListener("click", function () {
    DDLforChosenIllness.childNodes.forEach(opt => opt.selected = "selected");
    DDLforChosenConcern.childNodes.forEach(opt => opt.selected = "selected");
})