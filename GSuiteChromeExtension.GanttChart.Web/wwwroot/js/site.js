var SCOPES = 'https://www.googleapis.com/auth/drive.file';
var CLIENT_ID = $("#loader").attr("data-client-id");
var DISCOVERY_DOCS = ['https://www.googleapis.com/discovery/v1/apis/drive/v3/rest'];
var authLoaded = false, pickerLoaded = false;
var ganttChart;
var selectedTask;
var isEditTask;

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
        category: "Example"
    });

    ganttChart.Draw();

    $('[data-dismiss=modal]').on('click', function (e) {
        var $t = $(this),
            target = $t[0].href || $t.data("target") || $t.parents('.modal') || [];

        $(target)
            .find("input,textarea,select")
            .val('')
            .end()
            .find("input[type=checkbox], input[type=radio]")
            .prop("checked", "")
            .end();
    });
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

function onAddButtonClick() {
    $("#task-modal-action-btn").text("Add task");
    $("#task-modal-title").text("Add new task");
    isEditTask = false;
    initSelect2();
}

function onEditButtonClick() {
    $("#task-modal-action-btn").text("Save changes");
    $("#task-modal-title").text("Edit task");
    isEditTask = true;
    initSelect2();

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

//function validateModal() {
//    var result = true;
//    $.each($("#task-modal").find(":input").filter("[required]"), function (i, v) {
//        if (!v.value) {
//            result = false;
//            return;
//        }
//    });

//    return result;
//}

function modalAction(e) {
    e.preventDefault();

    var modal = $("#task-modal");

    var taskName = modal.find("input[name='task-name']").val();
    var taskType = modal.find("select[name='task-type']").val();
    var startDate = modal.find("input[name='task-start']").val();
    var endDate = modal.find("input[name='task-end']").val();
    var resourceName = modal.find("input[name='task-resource-name']").val();
    var dependency = modal.find("select[name='task-dependency']").val();
    var category = modal.find("input[name='task-category']").val();
    var note = modal.find("input[name='task-note']").val();
    var cost = modal.find("input[name='task-cost']").val();
    var completion = modal.find("input[name='task-completion']").val();
    var isMilestone = modal.find("input[name='task-milestone']")[0].checked;
    var parent = modal.find("select[name='task-parent']").val() || 0;

    if (isEditTask) {
        selectedTask.setName(taskName);
        selectedTask.setGroup(taskType);
        selectedTask.setStart(startDate);
        selectedTask.setEnd(endDate);
        selectedTask.setResource(resourceName);
        selectedTask.setDepend(dependency);
        selectedTask.getDataObject().category = category;
        selectedTask.getNotes().textContent = note;
        selectedTask.setCost(cost);
        selectedTask.setCompVal(completion);
    } else {
        var pClass = taskType === 1 ? "ggroupblack" : "gtaskblue";
        ganttChart.AddTaskItemObject({
            pID: 1,
            pName: taskName,
            pStart: startDate,
            pEnd: endDate,
            //pPlanStart: "2017-04-01",
            //pPlanEnd: "2017-04-15 12:00",
            pClass: pClass,
            //pLink: "",
            pMile: isMilestone ? 1 : 0,
            pRes: resourceName,
            pComp: completion,
            pGroup: taskType,
            pParent: parent,
            pOpen: 1,
            pDepend: dependency,
            pCaption: "",
            pCost: cost,
            pNotes: note,
            category: category
        });
    }

    ganttChart.Draw();

    $("#task-modal").find("button[type='reset']").trigger("click");
}

function initSelect2() {
    var dataSource = getSelect2TaskList();

    $("select[name='task-dependency']").select2({
        data: dataSource,
        multiple: true,
        width: "100%",
        containerCssClass: "form-control-border"
    });

    $("select[name='task-parent']").select2({
        data: dataSource,
        multiple: false,
        width: "100%",
        containerCssClass: "form-control-border"
    });
}

function setSelectedTaskLabel(name) {
    $("#select-task-label").text(name);
}

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

function getSelect2TaskList() {
    var result = [];
    ganttChart.vTaskList.forEach(function (v, i) {
        result.push({
            id: v.getID(),
            text: v.getName()
        });
    });

    return result;
}