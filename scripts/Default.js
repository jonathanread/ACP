
$(document).ready(function(){
    $(".wrapper .header .menu li")
        .mouseover(function () {
            $(this).children(".subMenu").show();
        })
        .mouseout(function () {
            $(this).children(".subMenu").hide();
        });
    if($(".wrapper .rotator").length){
        $(".wrapper .rotator").cycle({ cleartype: true, cleartypeNoBg: true, pause: true,height:"100%",width:"100%" });
    }
});