var authApiLoaded = false, pickerApiLoaded = false,
    driveReady = false;
var apiServer;

var googleOauthToken, selectingDriveFiles;

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
    }, onDriveAuthorized);
}

function onDriveAuthorized(authResult) {
    if (authResult && !authResult.error && authResult.access_token) {
        googleOauthToken = authResult.access_token;

        var picker = new google.picker.PickerBuilder()
            .addView(google.picker.ViewId.DOCS)
            .setOAuthToken(googleOauthToken)
            .setCallback(onDriveFileSelected)
            .build();

        picker.setVisible(true);
    } else {
        alert(authResult.error || "Error contacting Google Drive service.");
    }
}

function onDriveFileSelected(response) {
    if (response.action != "picked") {
        return;
    }

    selectingDriveFiles = response;
    var files = response.docs;
    $("#lbl-drive-files").html(files.length + " files selected.");
}

function onUnzipButtonClick() {
    try {
        setActionButtonEnabled(false);
        performUnzip()
            .then(response => {
                var url = "/unzip-files/" + encodeURI(response.Id);
                window.location.href = url;
            })
            .fail(xhr => {
                alert("There was an error unzipping: " + xhr.responseText);
                setActionButtonEnabled(true);
            });
    } catch (e) {
        alert(e);
        setActionButtonEnabled(true);
    }
}

function setActionButtonEnabled(enabled) {
    var buttons = $("#pnl-actions").find("button");

    if (enabled) {
        buttons.removeAttr("disabled");
        $("#lbl-working").addClass("invisible");
    } else {
        buttons.attr("disabled", "");
        $("#lbl-working").removeClass("invisible");
    }
}

function performUnzip() {
    var input = $("[name='opt-file-input']:checked").val();
    
    switch (input) {
        case "upload":
            {
                var files = $("#txt-upload-files")[0].files;
                if (!files || files.length == 0) {
                    throw "Please select files you want to zip.";
                }

                var data = new FormData();

                for (var file of files) {
                    data.append("File", file);
                }

                return $.ajax({
                    url: apiServer + "api/zip/unzip",
                    data: data,
                    method: "POST",
                    cache: false,
                    contentType: false,
                    processData: false,
                });
            }
        case "drive":
            {
                if (!selectingDriveFiles || !selectingDriveFiles.docs || !selectingDriveFiles.docs.length) {
                    throw "Please select files you want to zip.";
                }

                var data = {
                    OAuthToken: googleOauthToken,
                    File: selectingDriveFiles.docs[0],
                };

                return $.ajax({
                    url: apiServer + "api/zip/unzip-drive",
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    method: "POST",
                })
            }
        default:
            throw "Unknown Input";
    }
}

$(function () {
    setUIEnabled();
    apiServer = $("#loader").attr("data-server");

    $("[name='opt-file-input']").change(setUIEnabled);
    $("#btn-pick-drive").click(onDrivePickButtonClick);

    $("#btn-unzip").click(onUnzipButtonClick);
})