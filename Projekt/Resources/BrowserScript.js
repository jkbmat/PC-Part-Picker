$(document).ready(function () {
    buildView();
    window.external.DetectUpdateSize();
});

var EMPTY_BUILD_MESSAGE = "<br/><br/><br/><center>This build is currently empty.<br/><span class='noprint'>To start, <button onclick='window.external.FindPowerSource()'>Add a Power Source</button></span></center>";


// Refreshes the view
function buildView()
{
    if (window.external.IsEmpty())
    {
        $("#view").html(EMPTY_BUILD_MESSAGE);
        $("#energy").html("");
    }
    else
    {
        window.external.RefreshBrowser();
    }
}

// Sets the correct power usage
function setPower(source, current)
{
    var color = "green";

    if (current / -source > 0.75)
        color = "orange";
    if (current + source > 0)
        color = "red";
    var a = "<span style='color: " + color + "'>Currently using " + current + " W with a " + (-source) + " W power source</span>";
    
    $("#energy").html(a);
}

function setMissingComponents(components)
{
    if (components === "")
    {
        $("#missing-components").hide();
        return;
    }

    $("#missing-components").show();
    $("#missing-components").html("Your build is missing following required components: "+components)
}

// Function for external setting of view
function setView(html)
{
    $("#view").html(html);
}

// Connect button click
function addComponent(manufacturer, model, connector)
{
    window.external.FindComponentsByInConnector(connector, manufacturer, model);
}

// Open and close the details pane
function toggleDetails(manufacturer, model)
{
    var element = getComponent(manufacturer, model);
    if ($(element).find(".details").is(":visible"))
    {
        $(element).find(".toggle-details").text("▾ Details");
    }
    else
    {
        $(element).find(".toggle-details").text("▴ Details");
    }
    element.find(".details").toggle(100);
}

// Returns the element holding a component
function getComponent(manufacturer, model)
{
    return $("div[manufacturer='" + manufacturer + "'][model='" + model + "']");
}

// Remove buton click
function remove(manufacturer, model)
{
    if (!confirm("Are you sure you want to remove the "+manufacturer+" "+model+"?"))
        return;

    window.external.RemoveComponent(manufacturer, model);
    buildView();
}