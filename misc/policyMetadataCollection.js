var clientContext = new SP.ClientContext.get_current();
var oWebsite = clientContext.get_web();
var list_name = "policy";
var oList = oWebsite.get_lists().getByTitle(list_name);
var collListItems, list_length;

var subCats = ['101-130 Governance and Organization', '131-150 Institutional Operations', 
'151-170 Individual Rights', '201-220 General Financial', '221-240 Procurement and Travel',
'241-260 Auxiliary', '261-290 Development', '301-320 Hiring Practices',
'321-340 Conditions of Employment', '341-370 Compensation and Benefits',
'371-380 Disciplinary', '401-420 Health and Safety', '421-440 Emergency and Security',
'441-460 Facilities and Scheduling', '461-470 Information Technology',
'501-520 Admissions, Enrollment, Tuition, and Commencement',
'521-540 Academic Standards, Credits, and Grades', '541-550 Student Clubs',
'551-570 Student Rights', '601-630 Instruction and Curriculum', '631-660 Faculty'];

var cats = {100: "100: Administration",
200: "200: Financial Affairs and Development",
300: "300: Human Resources",
400: "400: Facilities, Operations, and Information Technology", 
500: "500: Students", 
600: "600: Academics",
700: "700: Graduate"
}


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
        var category = cats[parseInt(policyNumber[0]+ '00')];
        var subCategory = null;
        for (var j = 0; j < subCats.length; j++) {
            var subcat = subCats[j];
            var range = subcat.split(' ')[0].split('-');
            if (parseInt(range[0]) <= parseInt(policyNumber) && parseInt(policyNumber) <= parseInt(range[1])) {
                subCategory = subcat;
            }
        }
        var owner = list_item.get_item("Owner");
        var steward = list_item.get_item("Steward");
        var info = {
                727: policyNumber,
                734: title,
                732: category,
                742: subCategory,
                733: owner,
                741: steward,
            }
        metadata[policyNumber] = info;
    }
    console.log(JSON.stringify(metadata));
}