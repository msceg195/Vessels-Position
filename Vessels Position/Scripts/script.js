var myVar;

function loader() {
    myVar = setTimeout(showPage, 1000);
}

function showPage() {
    document.getElementById("loader").style.display = "none";
    document.body.style.backgroundImage = "none";
    document.getElementById("myDiv").style.display = "block";
}

$('#scrollbar').slimscroll();

var today = '@DateTime.Now.ToShortDateString()';

$('.table tr').each(function () {
    var _date = new Date($(this).children("td:eq(3)").val());
    var year = d.getFullYear();
    var month = d.getMonth();
    var day = d.getDate();

    var date = year + '/' + month + '/' + day;
});


$('html, body').animate({
    scrollTop: $('.today').offset().top
}, 800, function () {

});
