var apiServer;
var id, path;

function loadFileList() {
    $.ajax({
        url: apiServer + "api/zip/list",
        data: {
            id: id,
            path: path,
        },
        method: "GET"
    })
        .done(showItems)
        .fail(xhr => {
            alert("Failed to load the file. Please refresh the page and try again later. " +
                xhr.responseText);
        });
}

function showItems(data) {
    var list = $("#lst-files");
    list.html("");

    var fileTemplate = $("#template-file").html();
    var folderTemplate = $("#template-folder").html();

    for (var child of data.Children) {
        var dom;

        if (child.IsFolder) {
            dom = $(folderTemplate);

            var folderUrl = "/unzip-files/" + encodeURI(id) + "?path=" +
                encodeURIComponent(child.Path);
            dom.find("[data-link]").attr("href", folderUrl)
        } else {
            dom = $(fileTemplate);

            var downloadUrl = apiServer + "api/zip/download?id=" +
                encodeURIComponent(id) + "&path=" +
                encodeURIComponent(child.Path);

            dom.find("[data-link]").attr("href", downloadUrl);

            gapi.savetodrive.render(
                dom.find("[data-download-drive]")[0],
                {
                    src: downloadUrl,
                    filename: child.Name,
                    sitename: "Zip File Tab",
                }
            );
        }

        dom.find("[data-name]").html(child.Name);
        
        list.append(dom);
    }

    $("#lbl-loading").addClass("d-none");
}

$(function () {
    var loader = $("#loader"); 

    apiServer = loader.attr("data-server");
    id = loader.attr("data-id");
    path = loader.attr("data-path")

    loadFileList();
})