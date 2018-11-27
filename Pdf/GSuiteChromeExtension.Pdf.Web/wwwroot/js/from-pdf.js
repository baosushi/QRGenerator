function setUIEnabled() {
    var txtUploadFiles = $("#txt-upload-file");
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

function onDriveAuthorized(authResult) {
    if (authResult && !authResult.error && authResult.access_token) {
        googleOauthToken = authResult.access_token;

        showPdfDrivePicker(onDriveFileSelected);
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

function onConvertAndDownloadButtonClick() {
    try {
        setActionButtonEnabled(false);
        performUploadAndConvert()
            .then(response => {
                var url = getDownloadResultLink(response.id);
                window.location.href = url;

                setActionButtonEnabled(true);
            })
            .catch(xhr => {
                alert("There was an error converting: " + xhr.responseText);
                setActionButtonEnabled(true);
            });
    } catch (e) {
        alert(e);
        setActionButtonEnabled(true);
    }
}

function onConvertAndSaveToDriveButtonClick() {
    try {
        setActionButtonEnabled(false);
        performUploadAndConvert()
            .then(response => {
                showDownloadDriveDialog(
                    getDownloadResultLink(response.id),
                    response.outputFile,
                    "PDF Converted"
                );
            })
            .catch(xhr => {
                alert("There was an error converting: " + xhr.responseText);
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

function performUploadAndConvert() {
    return new Promise((resolve) => {
        performUpload()
            .done(response => {
                performConvert(response.id)
                    .done(resolve);
            })
            .fail(onFailedRequest);
    });
}

function performConvert(id) {
    var outputFormat = $("#cbo-output").find(":selected").val();
    
    return $.ajax({
        url: apiServer + "api/pdf/convert",
        data: {
            id: id,
            output: outputFormat,
        }
    });
}

$(function () {
    setUIEnabled();
    apiServer = $("#loader").attr("data-server");

    $("[name='opt-file-input']").change(setUIEnabled);
    $("#btn-pick-drive").click(onDrivePickButtonClick);

    $("#btn-convert-download").click(onConvertAndDownloadButtonClick);
    $("#btn-convert-save-drive").click(onConvertAndSaveToDriveButtonClick);
})