var clientContext = new SP.ClientContext.get_current();
var oWebsite = clientContext.get_web();
var list_name = "my-list";
var oList = oWebsite.get_lists().getByTitle(list_name);
var collListItems, list_length;

function loadFieldValues() {
    clientContext.load(oList);
    collListItems = oList.getItems("");
    clientContext.load(collListItems);
    /*
    **optional**
    var include = 'Include(comma-separated field names)';
    clientContext.load(collListItems, include);
    */
    clientContext.executeQueryAsync(successHandler, errorHandler);
    function successHandler() {
        console.log("Package Success.");
        setFieldValues()
    }
    function errorHandler() {
        console.log("Request failed: " + arguments[1].get_message());
    }
}
function setFieldValues() {
    list_length = oList.get_itemCount();
    for (var i = 0; i < list_length; i++) {
        var list_item = collListItems.getItemAtIndex(i);

        list_item.set_item("field-name", "field-value");
        list_item.update();
    }
    clientContext.executeQueryAsync(successHandler, errorHandler);
    function successHandler() {
        console.log("Metadata successfully updated.");
    }
    function errorHandler() {
        console.log("Request failed: " + arguments[1].get_message());
    }
}

