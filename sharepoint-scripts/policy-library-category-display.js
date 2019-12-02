function displayCat(catID, sub_nav) {
	var sub_cat = document.getElementsByClassName('sub-cat');
	for (var i = 0; i < sub_cat.length; i++) {
		sub_cat[i].style.display = 'none';
	}
	for (var i = 0; i < sub_nav.length; i++) {
		sub_nav[i].style.display = 'block';
	}
	var drop_down = document.getElementById('cat-dropdown')
	drop_down.getElementsByTagName('option')[0].selected = 'selected';
	drop_down.style.display = 'block';
	document.getElementById(catID).style.display = 'block';
};

document.getElementById("nav-100").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-100');
	displayCat('cat-100', sub_nav);
});
document.getElementById("nav-200").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-200');
	displayCat('cat-200', sub_nav);
});
document.getElementById("nav-300").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-300');
	displayCat('cat-300', sub_nav);
});
document.getElementById("nav-400").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-400');
	displayCat('cat-400', sub_nav);
});
document.getElementById("nav-500").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-500');
	displayCat('cat-500', sub_nav);
});
document.getElementById("nav-600").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-600');
	displayCat('cat-600', sub_nav);
});
document.getElementById("nav-700").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-700');
	displayCat('cat-700', sub_nav);
});
document.getElementById("nav-owners").addEventListener("click", function() {
	var sub_nav = document.getElementsByClassName('nav-owner');
	displayCat('cat-owners', sub_nav);
});