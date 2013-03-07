
$(document).ready(function(){
    $(".wrapper .header .menu li")
        .mouseover(function () {
            $(this).children(".subMenu").show();
        })
        .mouseout(function () {
            $(this).children(".subMenu").hide();
        });
    if($(".wrapper .rotator").length){
    	$(".wrapper .rotator").cycle({
    		cleartype: true,
    		cleartypeNoBg: true,
    		pause: true,
    		height: "100%",
    		width: "100%",
    		next: $(".next"),
    		prev: $(".prev"),
    		pager: $(".rotatorPager")
    	});
    }

    if ($("#contact").length) {
    	$(".ReqErrorMsg").hide();
    	$("#Phone").mask("(999) 999-9999? ext99999", { placeholder: "#" });
    	$("#ContactForm").submit(function () {
    		var formData = $(this).serialize().split("&");
    		return validateFields(formData);
    	});
    }
});

function validateFields(formData)
{
	var valid = true;
	var fields = [];
	var emRegEx = /^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$/;
	for (var i in formData) {
		var field = formData[i].split("=")[0].replace("=", "");
		fields.push(field);

		

		if (formData[i].split("=")[1].length == 0) {
			valid = false;
			$("#" + field).siblings(".ReqErrorMsg").show();
		}
		else {
			$("#" + field).siblings(".ReqErrorMsg").hide();
			if (field == "Email") {
				if (!emRegEx.test($("#" + field).val())) {
					valid = false;
					$("#" + field).siblings(".ReqErrorMsg").html("Invalid Email").show();
				}
				else {
					$("#" + field).siblings(".ReqErrorMsg").html("This field is required.").hide();
				}
			}
		}
		if (field == "RequestType") {
			if (formData[i].split("=")[1] == "Please+Select") {
				valid = false;
				$("#" + field).siblings(".ReqErrorMsg").show();
			}
			else {
				$("#" + field).siblings(".ReqErrorMsg").hide();
			}
		}
	}

	if (fields.indexOf("contactTime") == -1) {
		if ($("#contactTime").length) {

			$("#contactTime").siblings(".ReqErrorMsg").show();
		}
	} else {
		$("#contactTime").siblings(".ReqErrorMsg").hide();
	}

	if (fields.indexOf("contactMethod") == -1) {
		if ($("#contactMethod").length) {
			valid = false;
			$("#contactMethod").siblings(".ReqErrorMsg").show();
		}
	}
	else {
		$("#contactMethod").siblings(".ReqErrorMsg").hide();
	}
	$("#loader").hide();
	return valid;
}
