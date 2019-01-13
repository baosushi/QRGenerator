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
                    video[0].ontimeupdate = function () { videoPlayingHandler() };
                }
            });

            $("#slider-range").removeClass("d-none");

            startTime = 0;
            endTime = video[0].duration;
            $("#start-time").val(startTime);
            $("#end-time").val(endTime);
        }
    });

    video.append("<source src=\"" + "api/file/getFile?Token=" + oauthToken + "&Id=" + firstFile.id + "&Name=" + firstFile.name + "\" />");

    //$("#amount").val("$" + $("#slider-range").slider("values", 0) +
    //    " - $" + $("#slider-range").slider("values", 1));
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

$('#btn-save-to-drive').click(function () {
    gifshot.createGIF({
        'video': ['example.mp4']
    }, function (obj) {
        if (!obj.error) {
            var image = obj.image,
                animatedImage = document.createElement('img');
            animatedImage.src = image;
            document.body.appendChild(animatedImage);
        }
    });
});