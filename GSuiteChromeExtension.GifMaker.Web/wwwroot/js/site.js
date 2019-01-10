function getUserSelectedFile() {
    debugger
    var firstFile = driveFileInfo.docs[0];

    $.ajax({
        type: "GET",
        url: "https://www.googleapis.com/drive/v3/files/" + state.ids[0] + "?alt=media",
        beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'Bearer ' + oauthToken); },
        success: function (result) {
            userMap = JSON.parse(result);
            mapModel.setUserMap(userMap);
        },
        error: function (result) {
            console.log(result);
        }
    });
}