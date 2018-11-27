function createPicker() {
    var loader = $("#loader");
    var apiKey = loader.attr("data-api-key");
    var projectNumber = loader.attr("data-proj");

    var picker = new google.picker.PickerBuilder()
        .addView(google.picker.ViewId.DOCS)
        .setOAuthToken(googleOauthToken)
        .setDeveloperKey(apiKey)
        .setAppId(projectNumber)
        .setCallback(onPickerCallback)
        .enableFeature(google.picker.Feature.MULTISELECT_ENABLED)
        .build();

    picker.setVisible(true);
}

function onPickerCallback(response) {
    if (response.action != "picked") {
        return;
    }

    selectingDriveFiles = response;
    var files = response.docs;
    $("#lbl-drive-files").html(files.length + " files selected.");
}

function onZipAndDownloadButtonClick() {
    try {
        setActionButtonEnabled(false);
        performZip()
            .then(response => {
                var url = getDownloadResultLink(response.Id, response.Path);
                window.location.href = url;

                setActionButtonEnabled(true);
            })
            .fail(xhr => {
                alert("There was an error zipping: " + xhr.responseText);
                setActionButtonEnabled(true);
            });
    } catch (e) {
        alert(e);
        setActionButtonEnabled(true);
    }
}

function onZipAndSaveToDriveButtonClick() {
    try {
        setActionButtonEnabled(false);
        performZip()
            .then(response => {
                var btnSaveToDrive = $("#btn-save-to-drive");
                btnSaveToDrive.html("");

                gapi.savetodrive.render(
                    btnSaveToDrive[0],
                    {
                        src: getDownloadResultLink(response.Id, response.Path),
                        filename: response.Path,
                        sitename: "Zip File Tab",
                    }
                );

                $("#diag-save-to-drive").modal();

                setActionButtonEnabled(true);
            })
            .fail(xhr => {
                alert("There was an error zipping: " + xhr.responseText);
                setActionButtonEnabled(true);
            });
    } catch (e) {
        alert(e);
        setActionButtonEnabled(true);
    }
}

function getDownloadResultLink(id, path) {
    return apiServer + "api/zip/download?id=" +
        encodeURIComponent(id) + "&path=" +
        encodeURIComponent(path);
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

function performZip() {
    var input = $("[name='opt-file-input']:checked").val();
    var output = $("#cbo-output").find(":selected").val();

    switch (input) {
        case "upload":
            {
                var files = $("#txt-upload-files")[0].files;
                if (!files || files.length == 0) {
                    throw "Please select files you want to zip.";
                }

                var data = new FormData();

                data.append("Output", output);
                for (var file of files) {
                    data.append("Files", file);
                }

                return $.ajax({
                    url: apiServer + "api/zip/zip",
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
                    Files: selectingDriveFiles.docs,
                    Output: output,
                };

                return $.ajax({
                    url: apiServer + "api/zip/zip-drive",
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
    apiServer = $("#loader").attr("data-server");

    $("[name='opt-file-input']").change(setUIEnabled);

    $("#btn-zip-download").click(onZipAndDownloadButtonClick);
    $("#btn-zip-save-drive").click(onZipAndSaveToDriveButtonClick);
})