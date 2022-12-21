var clientContext = new SP.ClientContext.get_current();
var oWebsite = clientContext.get_web();

var doclib_names = ["2008-2010", "2011", "2012", "2013", "2014", "2015", "2016", "Blank"]
var doclib_index = 0;

var folders, folder, files;
var library_enumerator, file_enumerator;

function loadLibraryItems() {
    var docLib = oWebsite.get_lists().getByTitle(doclib_names[doclib_index]);
    clientContext.load(docLib);

    folders = docLib.getItems("");
    clientContext.load(folders);
    clientContext.executeQueryAsync(libraryLoadSuccess, errorHandler);
}
loadLibraryItems();

function processLibraryItem() {
    if (library_enumerator.moveNext()) {
        var library_item = library_enumerator.get_current();
        folder = library_item.get_folder();
        clientContext.load(folder);
        clientContext.executeQueryAsync(folderLoadSuccess, errorHandler);
    }
    else {
        doclib_index++;
        loadLibraryItems();
    }
}

function loadFiles() {
    files = folder.get_files();
    clientContext.load(files);
    clientContext.executeQueryAsync(filesLoadSuccess, errorHandler);
}

function processFile() {
    if (file_enumerator.moveNext()) {
        var file = file_enumerator.get_current();
        var file_name = file.get_name();
        docID = file_name.slice(0, 8);
        var file_item = file.get_listItemAllFields();
        grabMetadata(docID, file_item);
    }
    else {
        processLibraryItem();
    }
}
function grabMetadata(docID, file_item) {
    fetch("" + "/" + docID).then(function (response) {
        return response.json();
    }).then(function (metadata) {

        var syllabus_professor;
        if (metadata["Contributor"]) {
        syllabus_professor = metadata["Contributor"];
        }
        else {
            syllabus_professor = metadata["Contributors"];
        }

        var syllabus_title = metadata["Title"];
        var syllabus_call_number = metadata["LC Call Number"];
        var syllabus_subject_heading = metadata["LC Subject Headings"];

        file_item.set_item("Title", syllabus_title);
        file_item.set_item("Professor", syllabus_professor);
        file_item.set_item("LC_x0020_Call_x0020_Number", syllabus_call_number);
        file_item.set_item("LC_x0020_Subject_x0020_Heading", syllabus_subject_heading);

        file_item.update();

        clientContext.executeQueryAsync(updateSuccess, errorHandler);
    });
}

function libraryLoadSuccess() {
    console.log("Package Success.");
    library_enumerator = folders.getEnumerator();
    processLibraryItem();
}
function folderLoadSuccess() {
    console.log("Folder successfully loaded.");
    loadFiles();
}
function filesLoadSuccess() {
    console.log("Files successfully loaded.");
    file_enumerator = files.getEnumerator();
    processFile();
}
function updateSuccess() {
    console.log("Metadata successfully updated.");
    processFile();
}

function errorHandler() {
    console.log("Request failed: " + arguments[1].get_message());
}


