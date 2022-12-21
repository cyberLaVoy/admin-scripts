var clientContext = new SP.ClientContext.get_current();
var oWebsite = clientContext.get_web();
var list_name = "drafts";
var oList = oWebsite.get_lists().getByTitle(list_name);
var collListItems, list_length;

function loadFieldValues() {
    clientContext.load(oList);
    collListItems = oList.getItems("");
    clientContext.load(collListItems);
    /*
    var include = 'Include("Stakeholder+Input")';
    clientContext.load(collListItems, include);
    */
    clientContext.executeQueryAsync(successHandler, errorHandler);
    function successHandler() {
        console.log("Package Success.");
        setFieldValues();
    }
    function errorHandler() {
        console.log("Request failed: " + arguments[1].get_message());
    }
}
function setFieldValues() {
    metadata = {};
    list_length = oList.get_itemCount();
    for (var i = 0; i < list_length; i++) {
        var list_item = collListItems.getItemAtIndex(i);
        console.log(list_item);
        var policyNumber = list_item.get_item("Policy_x0023_");
        var title = list_item.get_item("Title");
        var action = list_item.get_item("Action") 
        var stage = list_item.get_item("Stage") 
        var owner = list_item.get_item("Owner");
        var steward = list_item.get_item("Steward");
        var contact = list_item.get_item("Contact");
        var requestSummary = list_item.get_item("Stakeholder_x0020_Input");
        var commentBegin = list_item.get_item("Comment_x0020_Period_x0020_Begins")
        var commentEnd = list_item.get_item("Comment_x0020_Period_x0020_Ends")
        var botApproval = list_item.get_item("BOT_x0020_Approval")
        var info = {
                756: action,
                754: policyNumber,
                755: title,
                758: owner,
                767: steward,
                764: contact,
                760: requestSummary,
                757: stage,
                776: "Yes",
                777: "Yes",
                778: "Yes",
                762: commentBegin,
                763: commentEnd,
                761: botApproval
            }
        metadata[policyNumber] = info;
    }
    console.log(JSON.stringify(metadata));
}