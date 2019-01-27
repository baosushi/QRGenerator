var SCOPES = 'https://www.googleapis.com/auth/drive.file';
var CLIENT_ID = $("#loader").attr("data-client-id");
var DISCOVERY_DOCS = ['https://www.googleapis.com/discovery/v1/apis/drive/v3/rest'];
var authLoaded = false, pickerLoaded = false;
var ganttChart;
var selectedTask;

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
        .setQuery("*.swf");

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

$(document).ready(function () {
    ganttChart = new JSGantt.GanttChart(document.getElementById('GanttChartDIV'), 'day');

    ganttChart.setOptions({
        vCaptionType: 'Complete',  // Set to Show Caption : None,Caption,Resource,Duration,Complete,
        vQuarterColWidth: 36,
        vDateTaskDisplayFormat: 'day dd month yyyy', // Shown in tool tip box
        vDayMajorDateDisplayFormat: 'mon yyyy - Week ww',// Set format to dates in the "Major" header of the "Day" view
        vWeekMinorDateDisplayFormat: 'dd mon', // Set format to display dates in the "Minor" header of the "Week" view
        vLang: 'en',
        vShowTaskInfoLink: 1, // Show link in tool tip (0/1)
        vShowEndWeekDate: 0,  // Show/Hide the date for the last day of the week in header for daily
        vAdditionalHeaders: { // Add data columns to your table
            category: {
                title: 'Category'
            },
            sector: {
                title: 'Sector'
            }
        },
        vUseSingleCell: 10000, // Set the threshold cell per table row (Helps performance for large data.
        vFormatArr: ['Day', 'Week', 'Month', 'Quarter'], // Even with setUseSingleCell using Hour format on such a large chart can cause issues in some browsers,
        vEventClickRow: onRowClick
    });

    // Load from a Json url
    JSGantt.parseJSON(dataJsonUrl, ganttChart);

    // Or Adding  Manually
    ganttChart.AddTaskItemObject({
        pID: 1,
        pName: "Define Chart API 2",
        pStart: "2017-02-25",
        pEnd: "2017-03-17",
        pPlanStart: "2017-04-01",
        pPlanEnd: "2017-04-15 12:00",
        pClass: "ggroupblack",
        pLink: "",
        pMile: 0,
        pRes: "Brian",
        pComp: 0,
        pGroup: 1,
        pParent: 0,
        pOpen: 1,
        pDepend: "",
        pCaption: "",
        pCost: 1000,
        pNotes: "Some Notes text",
        category: "Example",
        sector: "Finance"
    });

    ganttChart.Draw();

    function getTaskByName(name) {
        var task = null;
        try {
            ganttChart.vTaskList.forEach(function (v, i) {
                if (v.getName().toLowerCase() === name.toLowerCase()) {
                    task = v;
                }
            });
        } catch (e) {
            console.log(e);
        }

        return task;
    }

    function getTaskByOriginalId(id) {
        var task = null;
        try {
            ganttChart.vTaskList.forEach(function (v, i) {
                if (v.getID() === id) {
                    task = v;
                }
            });
        } catch (e) {
            console.log(e);
        }

        return task;
    }
});

function enableEditAndDelete() {
    if (selectedTask) {
        $("#delete-task").removeAttr("disabled");
        $("#edit-task").removeAttr("disabled");
    }
}

function disableEditAndDelete() {
    if (!selectedTask) {
        $("#delete-task").attr("disabled", "disabled");
        $("#edit-task").attr("disabled", "disabled");
    }
}

function onRowClick(task) {
    selectedTask = task;
    setSelectedTaskLabel(selectedTask.getName());
    enableEditAndDelete();
}

function onEditButtonClick() {

}

function onDeleteButtonClick() {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        ganttChart.RemoveTaskItem(selectedTask.getID());
        selectedTask = null;
        disableEditAndDelete();
        setSelectedTaskLabel('none');
        ganttChart.Draw();

        if (result.value) {
            Swal.fire(
                'Deleted!',
                'Your file has been deleted.',
                'success'
            );
        }
    });
}

function setSelectedTaskLabel(name) {
    $("#select-task-label").text(name);
}