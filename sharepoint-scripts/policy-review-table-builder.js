function buildPolicyReviewTable() {
	var source_list = "drafts";
	var des_list = "administration";

	var clientContext = new SP.ClientContext.get_current();
	var oWebsite = clientContext.get_web();
	var sourceList = oWebsite.get_lists().getByTitle(source_list);
	var destinationList = oWebsite.get_lists().getByTitle(des_list);
	var collListItems, listID, listLength;

	var docIDs = [];
	var leaf_ref = {};
	var policy_nums = {};
	var doc_titles = {};
	var actions = {};
	var stages = {};
	var owners = {};
	var stewards = {};
	var contacts = {};
	var stakeholder_inputs = {};
	var comment_period_begins = {};
	var comment_period_ends = {};
	var bot_approvals = {};

	var guest_links = {};
	var number_of_guest_links = 0;
	var properties_package = [];

	function loadProperties() {
		clientContext.load(sourceList, 'Id');
		clientContext.load(sourceList);
		clientContext.load(destinationList);
		collListItems = sourceList.getItems("");
		var include = 'Include(FileLeafRef,Id,Policy_x0023_,Title,Action,Stage,Owner,Steward,Contact,Stakeholder_x0020_Input,Comment_x0020_Period_x0020_Begins,Comment_x0020_Period_x0020_Ends,BOT_x0020_Approval)';
		clientContext.load(collListItems, include);
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
			leaf_ref[docID] = collListItems.getItemAtIndex(i).get_item("FileLeafRef");
		
			var policy_num = collListItems.getItemAtIndex(i).get_item("Policy_x0023_");
			valueCheck(policy_nums, policy_num);
			var title = collListItems.getItemAtIndex(i).get_item("Title");
			valueCheck(doc_titles, title);
			var action = collListItems.getItemAtIndex(i).get_item("Action");
			valueCheck(actions, action);
			var stage = collListItems.getItemAtIndex(i).get_item("Stage");
			valueCheck(stages, stage);
			var owner = collListItems.getItemAtIndex(i).get_item("Owner");
			valueCheck(owners, owner);
			var steward = collListItems.getItemAtIndex(i).get_item("Steward");
			valueCheck(stewards, steward);
			var contact = collListItems.getItemAtIndex(i).get_item("Contact");
			valueCheck(contacts, contact);
			var input = collListItems.getItemAtIndex(i).get_item("Stakeholder_x0020_Input");
			valueCheck(stakeholder_inputs, input);
			var begin = collListItems.getItemAtIndex(i).get_item("Comment_x0020_Period_x0020_Begins");
			valueCheck(comment_period_begins, begin, true);
			var end = collListItems.getItemAtIndex(i).get_item("Comment_x0020_Period_x0020_Ends");
			valueCheck(comment_period_ends, end, true);
			var approval = collListItems.getItemAtIndex(i).get_item("BOT_x0020_Approval");
			valueCheck(bot_approvals, approval, true);
			
			function valueCheck(obj_name, value, DATEVALUE=false) {
				if (value != null) {
					if (DATEVALUE) {
						obj_name[docID] = value.toLocaleDateString();
					}
					else {
						obj_name[docID] = value;
					}
				}
				else {
					if (DATEVALUE) {
						obj_name[docID] = "N/A";
					}
					else {
						obj_name[docID] = "";
					}
				}
			};
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
					buildTable();
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
		myRequest.onreadystatechange = function() {
			if (this.readyState == 4){
				if(this.status < 400 && this.status >= 200){
					console.log("Guest link created.");
					loadGuestLink(docID);
				}
				else{
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
			alert("Policy Review Table successfully updated.");
		}
		function errorHandler() {
			console.log("Request failed: " + arguments[1].get_message());
		}
	};
	function packageProperties() {
		properties_package.push("document.getElementById('insertDIV').innerHTML = " + '"' + "\\");
		properties_package.push("<table id='policyReview'>\\");
		properties_package.push("<tbody>\\");
		properties_package.push("<tr>\\");
		properties_package.push("<th data-cell='false' onclick='sortTable(0);'>Policy #</th>\\");
		properties_package.push("<th data-cell='false'>Title</th>\\");
		properties_package.push("<th data-cell='false' onclick='sortTable(2);'>Action</th>\\");
		properties_package.push("<th data-cell='false' onclick='sortTable(3);'>Stage</th>\\");
		properties_package.push("<th data-cell='false' onclick='sortTable(4);'>Owner</th>\\");
		properties_package.push("<th data-cell='false' onclick='sortTable(5);'>Steward</th>\\");
		properties_package.push("<th data-cell='false' onclick='sortTable(6);'>Contact</th>\\");
		properties_package.push("<th data-cell='false'>Stakeholder Input</th>\\");
		properties_package.push("<th data-cell='false'>Comment Period</th>\\");
		properties_package.push("<th data-cell='false'>BOT Approval</th>\\");
		properties_package.push("</tr>\\");
		for (var i = 0; i < docIDs.length; i++) {
			var docID = docIDs[i];
			properties_package.push("<tr>\\");
			properties_package.push("<td data-th='Policy #'>" + policy_nums[docID] + "</td>\\");
			properties_package.push("<td data-th='Title'>" + "<a href='" + guest_links[docID] + "'" + " target='_blank'>" + doc_titles[docID] + "</a>" + "</td>\\");
			properties_package.push("<td data-th='Action'>" + actions[docID] + "</td>\\");
			properties_package.push("<td data-th='Stage'>" + "<i>" + stages[docID] + "</i>" + "</td>\\");
			properties_package.push("<td data-th='Owner'>" + owners[docID] + "</td>\\");
			properties_package.push("<td data-th='Steward'>" + stewards[docID] + "</td>\\");
			properties_package.push("<td data-th='Contact'>" + contacts[docID] + "</td>\\");
			properties_package.push("<td data-th='Stakeholder Input'>" + stakeholder_inputs[docID] + "</td>\\");
			properties_package.push("<td data-th='Comment Period'>" + comment_period_begins[docID] + " - " + comment_period_ends[docID] + "</td>\\");
			properties_package.push("<td data-th='BOT Approval'>" + bot_approvals[docID] + "</td>\\");
			properties_package.push("</tr>\\");
		}
		properties_package.push("</tbody>\\");
		properties_package.push("</table>\\");
		properties_package.push("\\");
		properties_package.push('";');
		return properties_package;
	};
	function buildTable() {
		createFile("policyReview_tableFormat.js", packageProperties());
	}
	loadProperties();
};