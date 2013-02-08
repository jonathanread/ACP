
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
})
.bind("mobileinit", function () {
    $(".wrapper .header .menu li").tap(function(){
        if($(this).children(".subMenu").css("display") == "none"){
            $(this).children(".subMenu").show();
        }
        else{
            $(this).children(".subMenu").hide();
        }
    });
    $("#mobileAlert #close").tap(function () { $("#mobileAlert").hide();});
});