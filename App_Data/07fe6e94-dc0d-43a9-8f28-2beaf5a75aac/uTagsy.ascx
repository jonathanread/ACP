<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uTagsy.ascx.cs" Inherits="uTagsy.Web.usercontrols.uTagsy.datatypes.uTagsy" %>
<style type="text/css">
    /* containers */
    #uTagsy_leftCol, #uTagsy_middleCol, #uTagsy_rightCol { float: left; overflow: hidden; }
    #uTagsy_leftCol { padding-right: 30px; width: 200px;}
    #uTagsy_middleCol, #uTagsy_rightCol{padding-top:4px; overflow:hidden;}
    #uTagsy_middleCol{ height:20px; width: 100px;}
    #uTagsy_rightCol {width:300px}
    
    /* textbox*/
    #body_TabView1 .propertypane div.propertyItem #uTagsy input[type=textbox] { width: 150px; }
   
    /* dynamic tag list */
    #uTagsy_tags { list-style-type: none; }
    #uTagsy_tags li { padding-bottom: 5px; display: block; overflow: hidden; font-size:13px; }
    #uTagsy_tags a.uTagsy_delete { padding-right: 10px; text-decoration: none; }
    
    /* links */
    #uTagsy_add, #uTagsy_show_all, #uTagsy_hide_all { padding-left: 10px; text-decoration: none; color: #1541A9;}
    #uTagsy_add:hover, #uTagsy_show_all:hover, #uTagsy_hide_all:hover { text-decoration: underline; }
    
    /* all tags */
    #uTagsy_tags_all { list-style-type: none; margin:0; overflow:hidden; padding:0;}
    #uTagsy_tags_all li { display: block; float:left; padding-right: 10px; padding-bottom: 5px;height:15px; overflow:hidden; }
    #uTagsy_tags_all a.uTagsy_stored_tag {  text-decoration: none; font-size:16px; }
   
    #uTagsy_tags_all .uTagsy_stored_tag:hover { text-decoration: underline; }
    
    #uTagsy_tags a, #uTagsy_tags_all a {color: #1541A9;}
    #uTagsy_tags a:hover { text-decoration:underline;}
    
    /* autocomplete dropdown - copied from umbraco's standard tag styles */
    .ac_results { background-color: #F0F0F0; border: 1px solid #979797; overflow: hidden; padding: 3px; z-index: 99999; }
    .ac_results ul { list-style: none outside none; margin: 0; padding: 0; width: 100%; }
    .ac_results li { color: #1A1818; cursor: pointer; display: block; height: 20px; line-height: 20px; overflow: hidden; padding: 2px 2px 2px 5px; }
    .ac_results .ac_over { background-color: #D5EFFC; border: 1px solid #A8D8EB; padding: 1px 1px 1px 4px !important; }
</style>

<script src="/umbraco_client/Application/JQuery/jquery.autocomplete.js?cdv=1" type="text/javascript" language="javascript"></script>

<script type="text/javascript">
    $(document).ready(function () {
        var uTagsy = new function () {
            var document;

            this.tags = new Array();

            this.uTagsy_hfTags = $('input[id$=uTagsy_hfTags]');
            this.uTagsy_hfAllTags = $('input[id$=uTagsy_hfAllTags]');
            this.textbox = $('input[id$=uTagsy_txtTag]');
            this.landingId = $('input[id$=uTagsy_hfLandingId]').val();


            //--
            // Define Document class
            this.Document = function () {
                this.documentType = '<%= DocumentType %>';
                this.tags = new Array();
            },


            // bind enter press event for textbox
            this.textbox.keypress(function (e) {
                if (e.keyCode == 13) {
                    $('#uTagsy_add').trigger('click');
                    return false;
                }
            });

            //--
            // setup tags
            this.Init = function () {
                uTagsy.document = new uTagsy.Document();

                // get tags json text for this page
                var tagString = uTagsy.uTagsy_hfTags.val().toString().trim();
                if (tagString != '' && tagString != '""') {

                    // prefix tags
                    uTagsy.tags = [];
                    var tagNames = tagString.split(',');
                    for (var i = 0; i < tagNames.length; i++) {
                        uTagsy.tags.push(uTagsy.landingId + '_' + tagNames[i])
                    }

                    // create tags
                    for (var i = 0; i < uTagsy.tags.length; i++) {
                        uTagsy.createTagForNode(uTagsy.tags[i].replace(uTagsy.landingId + '_', ''));
                    }
                }

                // get tags json text for this document type
                var jsonText = uTagsy.uTagsy_hfAllTags.val().toString().trim();
                if (jsonText != '' && jsonText != '""') {
                    uTagsy.document = JSON.parse(jsonText.toString()).document;
                    for (i = 0; i < uTagsy.document.tags.length; i++) {
                        if (uTagsy.document.tags[i].indexOf(uTagsy.landingId + '_') == -1) {
                            // only display tags for this tree
                            continue;
                        }

                        uTagsy.createTagForDocumentType(uTagsy.document.tags[i].replace(uTagsy.landingId + '_', ''));

                        // add ',' separator for prettiness :)
                        if (i < uTagsy.document.tags.length - 1) {
                            $('#uTagsy_tags_all li:last').append(', ');
                        }
                    }
                }

            },


            //--
            // adds tag in current node's tag list
            this.createTagForNode = function (tag) {
                var link = $('<a class="uTagsy_delete" href="javascript:void(0);" rel="' + tag + '">[ x ]</a>').click(uTagsy.handleClickDelete);
                var listItem = $('<li></li>').append(link).append(tag).hide();
                $('#uTagsy_tags').append(listItem);
                $(listItem).fadeIn();
            },


            //--
            // adds tag to stored tag list
            this.createTagForDocumentType = function (tag) {
                var link = $('<a class="uTagsy_stored_tag" href="javascript:void(0);" rel="' + tag + '">' + tag + '</a>').click(uTagsy.handleStoredTagClick);
                var listItem = $('<li></li>').append(link);
                $('#uTagsy_tags_all').append(listItem);
            },



            //--
            // adds new tag to dom and tags array
            this.handleClickAdd = function () {

                // get tag from text box and trim
                var newTag = $.trim(uTagsy.textbox.val());

                var newTagPrefixed = uTagsy.landingId + "_" + newTag;

                // empty textbox, or tag already exists, so return
                if (newTag.length == 0 || $.inArray(newTagPrefixed, uTagsy.tags) != -1) {
                    return false;
                }

                // add tag to array
                uTagsy.tags.push(newTagPrefixed);

                // add tag to dom
                uTagsy.createTagForNode(newTag);

                // clear textbox
                uTagsy.textbox.val('');

                return false;
            },


            //--
            // deletes tag from dom and tag array
            this.handleClickDelete = function (sender) {
                // get tag name
                var tagToRemove = $(sender.target).attr('rel');

                var tagToRemovePrefixed = uTagsy.landingId + "_" + tagToRemove;

                // this check is probably redundant
                var tagIndex = $.inArray(tagToRemovePrefixed, uTagsy.tags);


                if (tagIndex != -1) {
                    // remove from array
                    // note: tag index may be -1 if json file was deleted
                    uTagsy.tags.splice(tagIndex, 1);
                }

                // get index of clicked tag
                var index = $('.uTagsy_delete').index(sender.target);

                // remove tag from dom
                $('#uTagsy_tags li').eq(index).fadeOut(function () {
                    $(this).remove();
                });

                return false;
            },



            //--
            // fills in textbox and triggers click to add tag
            this.handleStoredTagClick = function (sender) {

                // put value into textbox
                uTagsy.textbox.val($(sender.target).attr('rel'));

                // trigger click to add
                $('#uTagsy_add').trigger('click');
            },


            //--
            // Click event for uTagsy_add link
            $('#uTagsy_add').click(function () {
                uTagsy.handleClickAdd();
                return false;
            });


            //--
            // Click event for uTagsy_delete links
            $('.uTagsy_delete').click(function () {
                uTagsy.handleClickDelete(this);
                return false;
            });


            //--
            // Click event for uTagsy_show_all link to show all tags
            $('#uTagsy_show_all').click(function () {
                // show the tags
                $('#uTagsy_tags_all_container').fadeIn();

                // hide the show link
                $(this).hide();

                // show the hide link
                $('#uTagsy_hide_all').fadeIn();
                return false;
            });


            //--
            // Click event for uTagsy_hide_all link to hide all tags
            $('#uTagsy_hide_all').click(function () {
                // hide the tags
                $('#uTagsy_tags_all_container').hide();

                // hide the hide link
                $(this).hide();

                // show the show link
                $('#uTagsy_show_all').fadeIn();
                return false;
            });


            //--
            // Click event for uTagsy_stored_tag link to add tag
            $('.uTagsy_stored_tag').click(function () {
                handleStoredTagClick(this);
                return false;
            });


            //--
            // Binds click event of save and publish buttons.
            // Constructs json string and saves to input for saving.
            $('input[id$=layer_save],input[id$=layer_publish],input[id$=body_UnPublishButton],input[id$=layer_topublish]').click(function () {
                // sort tags alphabetically
                uTagsy.tags.sort();

                // save umbracoValue
                var jsonForUmbracoValue = JSON.stringify({ "tags": uTagsy.tags });
                uTagsy.uTagsy_hfTags.val(jsonForUmbracoValue);

                // add tags from uTagsy.tags to uTagsy.document.tags
                for (var i = 0; i < uTagsy.tags.length; i++) {
                    if ($.inArray(uTagsy.tags[i], uTagsy.document.tags) == -1) {
                        uTagsy.document.tags.push(uTagsy.tags[i]);
                    }
                }

                // sort tags alphabetically
                uTagsy.document.tags.sort();
                var jsonForSettings = JSON.stringify({ "document": uTagsy.document });
                $('input[id$=uTagsy_hfAllTags]').val(jsonForSettings);
            });

        } // end uTagsy

        uTagsy.Init();

        $('input[id$=uTagsy_txtTag]').autocomplete(uTagsy.document.tags);
    });
</script>

<div id="uTagsy">
    <asp:HiddenField ID="uTagsy_hfLandingId" runat="server" />
    <asp:HiddenField ID="uTagsy_hfAllTags" runat="server" />
    <asp:HiddenField ID="uTagsy_hfTags" runat="server" />
    <div id="uTagsy_leftCol">
        <asp:TextBox ID="uTagsy_txtTag" runat="server"></asp:TextBox><a id="uTagsy_add"
            href="javascript:void(0)">Add</a>
        <ul id="uTagsy_tags">
            <%--<li><a class="uTagsy_delete" href="javascript:void(0)">[x]</a>asdf</li>--%>
        </ul>
    </div>
    <div id="uTagsy_middleCol">
        <a id="uTagsy_show_all" href="javascript:void(0)">Show all tags</a>
        <a id="uTagsy_hide_all" style="display:none;" href="javascript:void(0)">Hide tags</a>
    </div>
    <div id="uTagsy_rightCol">
        <div id="uTagsy_tags_all_container" style="display: none;">
            <ul id="uTagsy_tags_all">
                <%--<li><a class="uTagsy_stored_tag" href="#" rel="sdf">sdf</a></li>--%>
            </ul>
        </div>
    </div>
</div>
