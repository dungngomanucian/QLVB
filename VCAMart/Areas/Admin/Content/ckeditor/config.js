/**
 * @license Copyright (c) 2003-2019, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see https://ckeditor.com/legal/ckeditor-oss-license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here. For example:
	// config.language = 'fr';
    // config.uiColor = '#AADC6E';

    config.enterMode = CKEDITOR.ENTER_DIV;
    config.extraPlugins = 'youtube,html5video';

    config.language = 'vi';
   
    config.height = 300;
    config.toolbarCanCollapse = true;
    config.filebrowserUploadMethod = 'form';
    config.filebrowserBrowseUrl = '/CKeditor/ckfinder/ckfinder.html';
    config.filebrowserImageBrowseUrl = '/CKeditor/ckfinder/ckfinder.html?type=Images';
    config.filebrowserFlashBrowseUrl = '/CKeditor/ckfinder/ckfinder.html?type=Flash';
    config.filebrowserUploadUrl = '/CKeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files';
    config.filebrowserImageUploadUrl = '/CKeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images';
    config.filebrowserFlashUploadUrl = '/CKeditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash';
    config.filebrowserWindowWidth = '1000';
    config.filebrowserWindowHeight = '700';
};
