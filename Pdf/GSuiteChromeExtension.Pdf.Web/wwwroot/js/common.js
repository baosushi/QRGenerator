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
    }, onDriveAuthorized);
}

function getDownloadResultLink(id) {
    return apiServer + "api/pdf/download?id=" +
        encodeURIComponent(id);
}

function performUpload() {
    var input = $("[name='opt-file-input']:checked").val();

    switch (input) {
        case "upload":
            {
                var files = $("#txt-upload-file")[0].files;
                if (!files || files.length == 0) {
                    throw "Please select files you want to convert.";
                }

                var data = new FormData();

                for (var file of files) {
                    data.append("Files", file);
                }

                return $.ajax({
                    url: apiServer + "api/pdf/upload",
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
                    throw "Please select files you want to convert.";
                }

                var data = {
                    OAuthToken: googleOauthToken,
                    Files: selectingDriveFiles.docs,
                };

                return $.ajax({
                    url: apiServer + "api/pdf/upload-drive",
                    data: JSON.stringify(data),
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    method: "POST",
                });
            }
        default:
            throw "Unknown Input";
    }
}

function onFailedRequest(xhr) {
    alert("Sorry there was a problem: " + xhr.responseText + ". Please try again later.");
    setActionButtonEnabled(true);
}

function showPdfDrivePicker(callback, allowMultiple) {
    var view = new google.picker.DocsView()
        .setMimeTypes("application/pdf");

    var pickerBuilder = new google.picker.PickerBuilder()
        .addView(view)
        .setSelectableMimeTypes("application/pdf")
        .setOAuthToken(googleOauthToken)
        .setCallback(callback);

    if (allowMultiple) {
        pickerBuilder = pickerBuilder
            .enableFeature(google.picker.Feature.MULTISELECT_ENABLED);
    }

    var picker = pickerBuilder.build();

    picker.setVisible(true);

    return picker;
}

function showDownloadDriveDialog(url, filename, sitename) {
    var btnSaveToDrive = $("#btn-save-to-drive");
    btnSaveToDrive.html("");

    gapi.savetodrive.render(
        btnSaveToDrive[0],
        {
            src: url,
            filename: filename,
            sitename: sitename,
        }
    );

    $("#diag-save-to-drive").modal();

    setActionButtonEnabled(true);
}