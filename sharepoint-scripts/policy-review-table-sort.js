function sortTable() {
    var table, rows, switching, i, x, y, shouldSwitch, a, b, c;
    a = "<i>Public Comment</i>"
    b = "<i>Awaiting Approval</i>"
    c = "<i>Approved</i>"
    table = document.getElementById("policyReview");
    switching = true;
    while (switching) {
        switching = false;
        rows = table.getElementsByTagName("TR");
        for (i = 1; i < rows.length - 1; i++) {
            shouldSwitch = false;
            x = rows[i].getElementsByTagName("TD")[3];
            y = rows[i + 1].getElementsByTagName("TD")[3];

            if (y.innerHTML == a && x.innerHTML == b || y.innerHTML == a && x.innerHTML == c) {
                shouldSwitch = true;
                break;
            }
            if (y.innerHTML == b && x.innerHTML == c) {
                shouldSwitch = true;
                break;
            }
        }
        if (shouldSwitch) {
            rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
            switching = true;
        }
    }
};
sortTable();