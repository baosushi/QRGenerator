var authApiLoaded = false, pickerApiLoaded = false,
    driveReady = false;
var apiServer;

var googleOauthToken, selectingDriveFiles;

function onAuthApiLoad() {
    authApiLoaded = true;
    checkPickerReady();
}

function onPickerApiLoad() {
    pickerApiLoaded = true;
    checkPickerReady();
}

function checkPickerReady() {
    if (authApiLoaded && pickerApiLoaded) {
        driveReady = true;
        setUIEnabled();
    }
}

function onGoogleApiLoaded() {
    gapi.load("auth2", onAuthApiLoad);
    gapi.load('picker', onPickerApiLoad);
}

function onDrivePickButtonClick() {
    var loader = $("#loader");
    var clientId = loader.attr("data-client-id");
    var scope = loader.attr("data-scope")

    gapi.auth2.authorize({
        client_id: clientId,
        scope: scope,
    }, function (authResult) {
        if (authResult && !authResult.error && authResult.access_token) {
            googleOauthToken = authResult.access_token;

            createPicker();
        } else {
            alert(authResult.error || "Error contacting Google Drive service.");
        }
    });
}

function setUIEnabled() {
    var txtUploadFiles = $("#txt-upload-files");
    var txtPickDrive = $("#btn-pick-drive");
    txtUploadFiles.attr("disabled", "");
    txtPickDrive.attr("disabled", "");

    if ($("[name='opt-file-input'][value='upload']:checked").length) {
        txtUploadFiles.removeAttr("disabled");
    }

    if ($("[name='opt-file-input'][value='drive']:checked").length) {
        if (driveReady) {
            txtPickDrive.removeAttr("disabled");
        }
    }
}

$(function() {
    apiServer = $("#loader").attr("data-server");

    $("[name='opt-file-input']").change(setUIEnabled);
    $("#btn-pick-drive").click(onDrivePickButtonClick);
});