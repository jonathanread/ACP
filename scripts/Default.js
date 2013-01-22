
$(document).ready(function(){
    $(".wrapper .header .menu li")
        .mouseover(function () {
            $(this).children(".subMenu").show();
        })
        .mouseout(function () {
            $(this).children(".subMenu").hide();
        });
    $(".wrapper .rotator").cycle({ cleartype: true, cleartypeNoBg: true, pause: 1 });
});