﻿var SCOPES = 'https://www.googleapis.com/auth/drive.file';
var CLIENT_ID = $("#loader").attr("data-client-id");
var DISCOVERY_DOCS = ['https://www.googleapis.com/discovery/v1/apis/drive/v3/rest'];
var authLoaded = false, pickerLoaded = false;

window.loginService = new LoginService(CLIENT_ID, SCOPES, DISCOVERY_DOCS);
window.driveService = new DriveService();

function onAuthApiLoad() {
    window.loginService.initClient(updateSigninStatus);
    authLoaded = true;
    checkAvailable();
    //checkDriveParams();
}

function onPickerApiLoad() {
    pickerLoaded = true;
    checkAvailable();
}

function onGoogleApiLoaded() {
    gapi.load('client:auth2', onAuthApiLoad);
    gapi.load('picker', onPickerApiLoad);
}

function checkAvailable() {
    if (authLoaded && pickerLoaded) {
        $("#btn-select-file").removeAttr("disabled");
    }
}

function updateSigninStatus(isSignedIn) {
    if (isSignedIn) {
        //var useremail = window.loginService.userProfile().getEmail();
        //is_auth(useremail);
        checkDriveParams();
    } else {
        loginService.signIn();
    }
}

function checkDriveParams() {
    var stateJson = getParameterByName("state");

    if (stateJson) {
        var state = JSON.parse(stateJson);
        oauthToken = loginService.getAccessToken();

        if (driveFileInfo) {
            driveFileInfo = { docs: [{}] };
        }

        if (!oauthToken) {
            loginService.signIn().then(function () {
                oauthToken = loginService.getAccessToken();

                driveFileInfo.docs[0] = {
                    id: state.ids[0],
                    name: "dummy.mp4"
                };

                getUserSelectedFile();
            });
        } else {
            driveFileInfo.docs[0] = {
                id: state.ids[0],
                name: "dummy.mp4"
            };

            getUserSelectedFile();
        }
    }
}

function onSelectFileButtonClick(callback) {
    oauthToken = loginService.getAccessToken();

    if (!oauthToken) {
        loginService.signIn().then(function () {
            oauthToken = loginService.getAccessToken();
            createPicker();
        });
    } else {
        createPicker();
    }
}

function createPicker() {
    var appId = $("[data-app-id]").attr("data-app-id");

    var docsView = new google.picker.DocsView()
        .setIncludeFolders(false)
        .setSelectFolderEnabled(false)
        .setQuery("*.mp4");

    var picker = new google.picker.PickerBuilder()
        .addView(docsView)
        .enableFeature(google.picker.Feature.NAV_HIDDEN)
        .setAppId(appId)
        .hideTitleBar()
        .setOAuthToken(oauthToken)
        .setCallback(pickerCallback)
        .build();

    picker.setVisible(true);
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

function pickerCallback(info) {
    if (info.action !== "picked" || !info.docs || !info.docs.length) {
        return;
    }

    driveFileInfo = info;
    $("#lbl-selected-filename").text(driveFileInfo.docs[0].name);
    getUserSelectedFile();
}

var loopControl = $("#loop");
var startTime = 0;
var endTime = 0;

function getUserSelectedFile() {
    var firstFile = driveFileInfo.docs[0];

    var video = $("#video");
    video.html("");
    video[0].removeEventListener("timeupdate", videoPlayingHandler);

    video.removeClass("d-none");

    video[0].addEventListener('loadeddata', function () {
        if (this.readyState >= 2) {
            $("#controls").removeClass("d-none");
            if ($("#btn-generate").hasClass("d-none")) {
                $("#btn-generate").removeClass("d-none");
            }

            $("#slider-range").html("");
            $("#slider-range").slider({
                range: true,
                min: 0,
                max: video[0].duration,
                values: [0, video[0].duration],
                slide: function (e, ui) {
                    startTime = ui.values[0];
                    endTime = ui.values[1];
                    $("#start-time").val(startTime);
                    $("#end-time").val(endTime);

                    var selectedDuration = ui.values[1] - ui.values[0];

                    //loop listener
                    video[0].removeEventListener("timeupdate", videoPlayingHandler);
                    video[0].ontimeupdate = function () { videoPlayingHandler(); };
                }
            });

            $("#slider-range").removeClass("d-none");

            startTime = 0;
            endTime = video[0].duration;
            $("#start-time").val(startTime);
            $("#end-time").val(endTime);

            gifshot.utils.default.setDefaultVideoDimensions(video[0].videoWidth, video[0].videoHeight);
        }
    });

    var source = $("<source src=\"" + "api/file/getFile?Token=" + oauthToken + "&Id=" + firstFile.id + "&Name=" + firstFile.name + "\" />");
    video.append(source);
    video[0].src = source[0].src;
}

function videoPlayingHandler() {
    var video = $("#video")[0];

    var isPlaying = !(video.paused || video.ended || video.seeking || video.readyState < video.HAVE_FUTURE_DATA);

    if (video.currentTime >= endTime || video.currentTime < startTime) {
        var scrollPrior = video.currentTime < startTime;

        if (isPlaying) {
            video.pause();
        }
        video.currentTime = startTime;
        if (isPlaying) {
            video.play();
        }
    }
}

function getParameterByName(name) {
    var url = window.location.href;
    name = name.replace(/[\[\]]/g, '\\$&');
    var regex = new RegExp('[?&]' + name + '(=([^&#]*)|&|#|$)'),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, ' '));
}

$('#btn-generate').click(function () {
    var video = $("#video")[0];
    $("#result-container").html("");
    $("#result-container").append('<img src="/images/loading.gif" style="display: block; width: auto; margin: auto;" />');
    if (!$("#btn-download").hasClass("d-none")) {
        $("#btn-download").addClass("d-none");
    }
    if (!$("#btn-generate").hasClass("d-none")) {
        $("#btn-generate").addClass("d-none");
    }
    gifshot.createGIF({
        gifWidth: video.videoWidth,
        gifHeight: video.videoHeight,
        video: [
            $("#video")[0].src
        ],
        interval: 0.1,
        offset: startTime,
        numFrames: Math.ceil(endTime - startTime) / 0.1,
        frameDuration: 1
    }, function (obj) {
        if (!obj.error) {
            var image = obj.image,
                animatedImage = document.createElement('img');
            animatedImage.src = image;
            animatedImage.id = "result";
            $("#result-container").html("");
            $("#result-container").append(animatedImage);
            $("#btn-download").removeClass("d-none");
            $("#btn-generate").removeClass("d-none");
        }
    });
});

$("#btn-download").click(function () {
    var array = _base64ToArrayBuffer($("img#result")[0].src.replace("data:image/gif;base64,", ""));
    var image = new Blob([array], { type: "image/gif" });
    var url = URL.createObjectURL(image);

    var a = $("<a>")
        .attr("href", url)
        .attr("download", "generated-gif.gif")
        .appendTo("body");
    a[0].click();
    a.remove();
});

$(function () {
    $("#btn-select-file").click(onSelectFileButtonClick);
    $("#btn-save-to-drive").click(onSaveButtonClick);
});

function _base64ToArrayBuffer(base64) {
    var binary_string = window.atob(base64);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
}