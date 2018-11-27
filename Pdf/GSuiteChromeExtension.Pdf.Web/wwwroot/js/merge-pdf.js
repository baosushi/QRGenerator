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

        showPdfDrivePicker(onDriveFileSelected, true);
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

    showFileList();
}

function clearFileList(uiOnly) {
    var lstFiles = $("#list-files");
    lstFiles.html("");

    if (uiOnly !== true) {
        $("#txt-upload-file").val("");

        selectingDriveFiles = null;
        $("#lbl-drive-files").html(getLanguageText("File_NoFile"))
    }

    return lstFiles;
}

function showFileList() {
    var lstFiles = clearFileList(true);

    var template = $("#template-file-item").html();

    var addFile = function (index, name) {
        var dom = $(template);

        dom.find("[data-name]").html(name);
        dom.attr("data-index", index);

        lstFiles.append(dom);
    };

    var selectedInput = $("[name='opt-file-input']:checked").val();

    switch (selectedInput) {
        case "upload":
            var files = $("#txt-upload-file")[0].files;
            for (var i = 0; i < files.length; i++) {
                addFile(i, files[i].name);
            }

            break;
        case "drive":
            var files = selectingDriveFiles.docs;
            for (var i = 0; i < files.length; i++) {
                addFile(i, files[i].name);
            }

            break;
    }
}

function onFileItemActionButtonClick() {
    var e = $(this);
    var action = e.attr("data-list-action");
    var parent = e.closest(".item");

    switch (action) {
        case "up":
            var prev = parent.prev();
            if (prev.length) {
                prev.insertAfter(parent);
            }

            break;
        case "down":
            var next = parent.next();

            if (next.length) {
                next.insertBefore(parent);
            }

            break;
        case "delete":
            parent.remove();
            break;
    }
}

function onMergeAndDownloadButtonClick() {
    try {
        setActionButtonEnabled(false);
        performUploadAndMerge()
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

function onMergeAndSaveToDriveButtonClick() {
    try {
        setActionButtonEnabled(false);
        performUploadAndMerge()
            .then(response => {
                showDownloadDriveDialog(
                    getDownloadResultLink(response.id),
                    "merged.pdf",
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

function performUploadAndMerge() {
    return new Promise((resolve) => {
        performUploadForMerging()
            .done(response => {
                performConvert(response.id)
                    .done(resolve);
            })
            .fail(onFailedRequest);
    });
}

function performUploadForMerging() {
    var indexes = [];
    $("#list-files").find("[data-index]")
        .each(function () {
            indexes.push(Number($(this).attr("data-index")));
        });

    if (indexes.length == 0) {
        throw getLanguageText("File_NoFileSelectedError");
        return;
    }

    var input = $("[name='opt-file-input']:checked").val();

    switch (input) {
        case "upload":
            {
                var files = $("#txt-upload-file")[0].files;
                if (!files || files.length == 0) {
                    throw "Please select files you want to convert.";
                }

                var data = new FormData();

                for (var i of indexes) {
                    data.append("Files", files[i]);
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

                var files = [];
                for (var i of indexes) {
                    files.push(selectingDriveFiles.docs[i]);
                }

                var data = {
                    OAuthToken: googleOauthToken,
                    Files: files,
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

function performConvert(id) {
    return $.ajax({
        url: apiServer + "api/pdf/manipulate",
        data: {
            id: id,
            operation: "combine",
        }
    });
}

$(function () {
    setUIEnabled();
    apiServer = $("#loader").attr("data-server");

    $("[name='opt-file-input']").change(setUIEnabled);
    $("#btn-pick-drive").click(onDrivePickButtonClick);

    $("#btn-convert-download").click(onMergeAndDownloadButtonClick);
    $("#btn-convert-save-drive").click(onMergeAndSaveToDriveButtonClick);

    $("#txt-upload-file").change(showFileList);
    $("[name='opt-file-input']").change(() => clearFileList(false));

    $("#list-files").on("click", "[data-list-action]", onFileItemActionButtonClick);
})