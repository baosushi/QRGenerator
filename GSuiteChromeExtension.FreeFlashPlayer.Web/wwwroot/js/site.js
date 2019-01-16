var loopControl = $("#loop");
var startTime = 0;
var endTime = 0;

function getUserSelectedFile() {
    var firstFile = driveFileInfo.docs[0];

    var flashContainer = $("#flash-container");
    flashContainer.html("");

    var embed = $("<embed type=\"application/x-shockwave-flash\" src=\"" + "api/file/getFile?Token=" + oauthToken + "&Id=" + firstFile.id + "&Name=" + firstFile.name + "\" />");
    flashContainer.append(embed);

    if (flashContainer.hasClass("d-none")) {
        flashContainer.removeClass("d-none");
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
    var a = $("<a>")
        .attr("href", $("img#result")[0].src)
        .attr("download", "generated-gif.gif")
        .appendTo("body");
    a[0].click();
    a.remove();
});

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
                    id: state.ids[0],
                    name: "dummy.mp4"
                };

                getUserSelectedFile();
            }
        });
    }
});