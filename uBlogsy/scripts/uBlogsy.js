$(document).ready(function(){

    $('#uBlogsyBtnSearch').click(function(){
        @{var landing = DataService.Instance.GetLanding(Model.Id); }
        window.location.href = "@landing.Url" + '?search=' + $("#uBlogsyTxtSearch").val();
        return false;
    });

    $('#uBlogsyTxtSearch').click(function(){
        if ($(this).val() == 'Search'){
            $(this).val(''); 
        };
    });

    $('#uBlogsyTxtSearch').blur(function(){
        if ($(this).val() == ''){
            $(this).val('Search'); 
        };
    });
});

