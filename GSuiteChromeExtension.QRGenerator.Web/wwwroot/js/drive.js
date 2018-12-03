var authLoaded, pickerLoaded;
var oauthToken;
var driveFileInfo;

function onAuthApiLoad() {
    authLoaded = true;
    checkAvailable();
}

function onPickerApiLoad() {
    pickerLoaded = true;
    checkAvailable();
}

function onSelectFileButtonClick() {
    var clientId = $("#loader").attr("data-client-id");
    var scope = "https://www.googleapis.com/auth/drive.file";

    gapi.auth2.authorize({
        client_id: clientId,
        scope: scope
    }, handleAuthResult);
}

function handleAuthResult(authResult) {
    if (authResult && !authResult.error) {
        oauthToken = authResult.access_token;
        createPicker();
    }
}

function createPicker() {
    var developerKey = $("[data-developer-key]").attr("data-developer-key");
    var appId = $("[data-app-id]").attr("data-app-id");

    var docsView = new google.picker.DocsView();

    if ($("input[name='generate-type']:checked").val() === "folder") {
        docsView = docsView.setMimeTypes('application/vnd.google-apps.folder').setIncludeFolders(true).setSelectFolderEnabled(true);;
    } else {
        docsView = docsView.setIncludeFolders(false).setSelectFolderEnabled(false);
    }

    var picker = new google.picker.PickerBuilder()
        .addView(docsView)
        .enableFeature(google.picker.Feature.NAV_HIDDEN)
        .setAppId(appId)
        .hideTitleBar()
        .setOAuthToken(oauthToken)
        .setCallback(pickerCallback)
        .build();

    //var picker = new google.picker.PickerBuilder()
    //    .addView(google.picker.ViewId.DOCS)
    //    .setAppId(appId)
    //    .setDeveloperKey(developerKey)
    //    .setOAuthToken(oauthToken)
    //    .setCallback(pickerCallback)
    //    .build();
    picker.setVisible(true);
}

function pickerCallback(info) {
    if (info.action !== "picked" || !info.docs || !info.docs.length) {
        return;
    }

    driveFileInfo = info;
    var firstFile = info.docs[0];
    $("#lbl-selected-filename").html(firstFile.name);

    $("#txt-filename").removeAttr("disabled");
    $("#btn-save-to-drive").removeAttr("disabled");
}

function onSaveButtonClick() {
    $.ajax({
        url: "/api/generator/generate",
        method: "POST",
        contentType: "application/json",
        data: JSON.stringify({
            Token: oauthToken,
            File: driveFileInfo.docs[0],
            IsFolder: driveFileInfo.docs[0].type === "folder",
            WithDescription: $("#with-description").is(":checked"),
            Repetition: $("#repetition").val()
        })
    }).done(response => {
        var filename = driveFileInfo.docs[0].id + ".docx";

        gapi.savetodrive.render("btn-drive-button", {
            src: response.path,
            filename: filename,
            sitename: 'QRGenerator'
        });
    });
}

function checkAvailable() {
    if (authLoaded && pickerLoaded) {
        $("#btn-select-file").removeAttr("disabled");
    }
}

function onGoogleApiLoaded() {
    // Use the API Loader script to load the Authentication script.
    gapi.load('auth', { 'callback': onAuthApiLoad });

    // Use the API Loader script to load the google.picker script.
    gapi.load('picker', { 'callback': onPickerApiLoad });
}

$(function () {
    $("#btn-select-file").click(onSelectFileButtonClick);
    $("#btn-save-to-drive").click(onSaveButtonClick)
});