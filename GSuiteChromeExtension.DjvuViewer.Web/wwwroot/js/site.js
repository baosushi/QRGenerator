var SCOPES = 'https://www.googleapis.com/auth/drive.file';
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
                    id: state.ids[0]
                };

                getUserSelectedFile();
            });
        } else {
            driveFileInfo.docs[0] = {
                id: state.ids[0]
            };

            getUserSelectedFile();
        }
    }
}













var loopControl = $("#loop");
var startTime = 0;
var endTime = 0;

function getUserSelectedFile() {
    var firstFile = driveFileInfo.docs[0];

    var url = `api/file/getFile?Token=${oauthToken}&Id=${firstFile.id}`;
    var promise = window.ViewerInstance.loadDocumentByUrl(url);
    Promise.resolve(promise);
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

//$("#btn-download").click(function () {
//    var a = $("<a>")
//        .attr("href", $("img#result")[0].src)
//        .attr("download", "generated-gif.gif")
//        .appendTo("body");
//    a[0].click();
//    a.remove();
//});

function createPicker() {
    var developerKey = $("[data-developer-key]").attr("data-developer-key");
    var appId = $("[data-app-id]").attr("data-app-id");

    var docsView = new google.picker.DocsView()
        .setIncludeFolders(false)
        .setSelectFolderEnabled(false)
        .setQuery("*.djvu");

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

function pickerCallback(info) {
    if (info.action !== "picked" || !info.docs || !info.docs.length) {
        return;
    }

    driveFileInfo = info;

    getUserSelectedFile();
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

$(document).ready(function () {
    var stateJson = getParameterByName("state");

    if (stateJson) {
        var state = JSON.parse(stateJson);

        onSelectFileButtonClick(function (authResult) {
            if (authResult && !authResult.error) {
                oauthToken = authResult.access_token;

                if (driveFileInfo) {
                    driveFileInfo = { docs: [{}] };
                }

                driveFileInfo.docs[0] = {
                    id: state.ids[0]
                };

                getUserSelectedFile();
            }
        });
    }
});

$(function () {
    $("#btn-select-file").click(onSelectFileButtonClick);
});