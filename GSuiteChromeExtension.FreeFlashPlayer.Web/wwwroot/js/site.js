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