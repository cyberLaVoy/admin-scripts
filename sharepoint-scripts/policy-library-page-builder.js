function buildPolicyLibraryPage() {
    //add subcategories as they are created in the format 'xxx-xxx Category Name'
    var subCategories = ['101-130 Governance and Organization', '131-150 Institutional Operations',
        '151-170 Individual Rights', '201-220 General Financial', '221-240 Procurement & Travel',
        '241-260 Auxiliary', '261-290 Development', '301-320 Hiring Practices',
        '321-340 Conditions of Employment', '341-370 Compensation and Benefits',
        '371-380 Disciplinary', '401-420 Health and Safety', '421-440 Emergency and Security',
        '441-460 Facilities and Scheduling', '461-470 Information Technology',
        '501-520 Admissions, Enrollment, Tuition, and Commencement',
        '521-540 Academic Standards, Credits, and Grades', '541-550 Student Clubs',
        '551-570 Student Rights', '601-630 Instruction and Curriculum', '631-660 Faculty'];

    var clientContext = new SP.ClientContext.get_current();
    var oWebsite = clientContext.get_web();
    var source_list = "policy";
    var sourceList = oWebsite.get_lists().getByTitle(source_list);
    var des_list = "administration";
    var destinationList = oWebsite.get_lists().getByTitle(des_list);

    var subCat = {};
    var collListItems, listID, listLength;
    var docIDs = [];
    var leaf_ref = {};
    var doc_titles = {};
    var policy_nums = {};
    var cat_owner = {};
    var cat_subcategory = {};
    var guest_links = {};
    var number_of_guest_links = 0;

    var cat100 = ["<div id='cat-100' class='sub-cat'>"];
    var cat200 = ["<div id='cat-200' class='sub-cat'>"];
    var cat300 = ["<div id='cat-300' class='sub-cat'>"];
    var cat400 = ["<div id='cat-400' class='sub-cat'>"];
    var cat500 = ["<div id='cat-500' class='sub-cat'>"];
    var cat600 = ["<div id='cat-600' class='sub-cat'>"];
    var cat700 = ["<div id='cat-700' class='sub-cat'>"];
    var catOwner = ["<div id='cat-owners' class='sub-cat'>"];


    function loadProperties() {
        clientContext.load(sourceList);
        clientContext.load(sourceList, 'Id');
        clientContext.load(destinationList);
        collListItems = sourceList.getItems("");
        clientContext.load(collListItems, 'Include(FileLeafRef, Id, Title, Owner, Policy_x0023_)');
        clientContext.executeQueryAsync(successHandler, errorHandler);
        function successHandler() {
            console.log("Properties loaded.");
            logProperties();
        }
        function errorHandler() {
            console.log("Request failed: " + arguments[1].get_message());
        }
    };
    function logProperties() {
        listID = sourceList.get_id();
        listLength = sourceList.get_itemCount();
        for (var i = 0; i < listLength; i++) {
            var docID = collListItems.getItemAtIndex(i).get_id();
            docIDs.push(docID);
            doc_titles[docID] = collListItems.getItemAtIndex(i).get_item("Title");
            policy_nums[docID] = collListItems.getItemAtIndex(i).get_item("Policy_x0023_");
            cat_owner[docID] = collListItems.getItemAtIndex(i).get_item("Owner");
            leaf_ref[docID] = collListItems.getItemAtIndex(i).get_item("FileLeafRef");

            var num = collListItems.getItemAtIndex(i).get_item("Policy_x0023_").toString();
            var index = subCheck(num);
            cat_subcategory[docID] = subCategories[index];
        }
        function subCheck(num) {
            for (var j = 0; j < subCategories.length; j++) {
                var lower = parseInt(subCategories[j].slice(0, 3));
                var upper = parseInt(subCategories[j].slice(4, 7));
                var num_check = parseInt(num.slice(0, 3));
                if (num_check >= lower && num_check <= upper) {
                    return j
                }
            }
        }
        for (var i = 0; i < subCategories.length; i++) {
            subCat[subCategories[i]] = [];
        }
        for (var docID in cat_owner) {
            subCat[cat_owner[docID]] = [];
        }
        for (var i = 0; i < docIDs.length; i++) {
            loadGuestLink(docIDs[i]);
        }
    };
    function loadGuestLink(docID) {
        var sharedObj = new SP.ObjectSharingInformation.getListItemSharingInformation(clientContext, listID, docID, true, true, true, true, false, false);
        clientContext.load(sharedObj, 'AnonymousViewLink');
        clientContext.executeQueryAsync(successHandler, errorHandler);
        function successHandler() {
            console.log("Guest link loaded.");
            var guestLink = sharedObj.get_anonymousViewLink();
            if (guestLink != "") {
                guest_links[docID] = guestLink;
                number_of_guest_links++;
                if (number_of_guest_links == docIDs.length) {
                    buildCat();
                }
            }
            else {
                createGuestLink(docID);
            }
        }
        function errorHandler() {
            console.log("Request failed: " + arguments[1].get_message());
        }
    };
    function createGuestLink(docID) {
        var methodURL = _spPageContextInfo.webAbsoluteUrl + '/_api/SP.Web.CreateAnonymousLink';
        var docLocation = "/" + source_list + "/" + leaf_ref[docID];
        var docURL = _spPageContextInfo.webAbsoluteUrl + docLocation;
        var data = JSON.stringify({ 'url': docURL, 'isEditLink': false });
        var myRequest = new XMLHttpRequest();
        myRequest.onreadystatechange = function () {
            if (this.readyState == 4) {
                if (this.status < 400 && this.status >= 200) {
                    console.log("Guest link created.");
                    loadGuestLink(docID);
                }
                else {
                    console.log("Guest link creation failure.");
                }
            }
        }
        myRequest.open("POST", methodURL);
        myRequest.setRequestHeader("Content-Type", "application/json;odata=verbose");
        myRequest.setRequestHeader("Accepts", "application/json;odata=verbose");
        myRequest.setRequestHeader("X-RequestDigest", __REQUESTDIGEST.value);
        myRequest.send(data);
    };
    function createFile(file_leaf_ref, file_content) {
        var fileCreateInfo = new SP.FileCreationInformation();
        fileCreateInfo.set_url(file_leaf_ref);
        fileCreateInfo.set_content(new SP.Base64EncodedByteArray());
        fileCreateInfo.set_overwrite(true);
        for (var i = 0; i < file_content.length; i++) {
            for (var j = 0; j < file_content[i].length; j++) {
                fileCreateInfo.get_content().append(file_content[i].charCodeAt(j));
            }
            fileCreateInfo.get_content().append('10');
        }
        existingFile = destinationList.get_rootFolder().get_files().add(fileCreateInfo);
        clientContext.executeQueryAsync(successHandler, errorHandler);
        function successHandler() {
            alert("Policy Library successfully updated.");
        }
        function errorHandler() {
            console.log("Request failed: " + arguments[1].get_message());
        }
    };
    function processSubCat(docID) {
        for (var cat in subCat) {
            if (cat_subcategory[docID] == cat) {
                subCat[cat].push("<div id='" + policy_nums[docID] + "' class='link-wrap'><a href='" + guest_links[docID] + "' target='_blank' class='policy-link'>" + policy_nums[docID] + ": " + doc_titles[docID] + "</a></div>");
            }
            if (cat_owner[docID] == cat) {
                subCat[cat].push("<div id='" + policy_nums[docID] + "' class='link-wrap'><a href='" + guest_links[docID] + "' target='_blank' class='policy-link'>" + policy_nums[docID] + ": " + doc_titles[docID] + "</a></div>");
            }
        }
    };
    function processCat(sub_cat) {
        if (isNaN(parseInt(sub_cat[0]))) {
            logData(catOwner);
            return;
        }
        logData(eval('cat' + sub_cat[0] + '00'));
        function logData(cat_list) {
            cat_list.push("<div id='" + sub_cat + "'><div class='cat-wrap'><span class='cat-title'>" + sub_cat + "</span></div>");
            subCat[sub_cat].sort();
            for (var i = 0; i < subCat[sub_cat].length; i++) {
                cat_list.push(subCat[sub_cat][i]);
            }
            cat_list.push("</div>");
        }
    };
    function buildPage(cat_include) {
        var page_package = [];
        page_package.push("document.getElementById('link-list').innerHTML = " + '"' + "\\")
        for (var i = 0; i < cat_include.length; i++) {
            if (isNaN(parseInt(cat_include[i]))) {
                logData(catOwner);
            }
            switch (cat_include[i]) {
                case 1:
                    logData(cat100)
                    break;
                case 2:
                    logData(cat200)
                    break;
                case 3:
                    logData(cat300)
                    break;
                case 4:
                    logData(cat400)
                    break;
                case 5:
                    logData(cat500)
                    break;
                case 6:
                    logData(cat600)
                    break;
                case 7:
                    logData(cat700)
                    break;
            }
        }
        function logData(cat_list) {
            for (var j = 0; j < cat_list.length; j++) {
                page_package.push(cat_list[j] + "\\");
            }
        };
        page_package.push('";');
        function createDropdown() {
            page_package.push("document.getElementById('dropdown').innerHTML = " + '"' + "\\");
            page_package.push("<div id='categoryselection'><form name=catselect><select onchange='showCategory()' name='catdropdown' id='cat-dropdown'>\\");
            page_package.push("<option value='novalue' selected>Select a Subcategory</option>\\");
            for (var k = 0; k < cat_include.length; k++) {
                for (var cat in subCat) {
                    if (cat_include[k].toString() == cat[0]) {
                        page_package.push("<option class='sub-cat nav-all nav-" + cat.slice(0, 1) + "00' value='#" + cat + "'>" + cat + "</option>\\");
                    }
                    if (isNaN(parseInt(cat_include[k])) && isNaN(parseInt(cat[0]))) {
                        page_package.push("<option class='sub-cat nav-owner' value='#" + cat + "'>" + cat + "</option>\\");
                    }
                }
            }
            page_package.push("</select></form></div>\\");
            page_package.push('";');
        };
        createDropdown();
        return page_package;
    };
    function buildCat() {
        for (var i = 0; i < docIDs.length; i++) {
            processSubCat(docIDs[i]);
        }
        for (var sub_cat in subCat) {
            processCat(sub_cat);
        }
        cat100.push("</div>");
        cat200.push("</div>");
        cat300.push("</div>");
        cat400.push("</div>");
        cat500.push("</div>");
        cat600.push("</div>");
        cat700.push("</div>");
        catOwner.push("</div>");
        createFile("policyLibrary_homePage.js", buildPage([1, 2, 3, 4, 5, 6, 7, "o"]));
    };
    loadProperties();
};