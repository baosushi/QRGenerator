function slugify(text) {
    return text.toString().toLowerCase()
        .replace(/\s+/g, '-')           // Replace spaces with -
        .replace(/[^\w\-]+/g, '')       // Remove all non-word chars
        .replace(/\-\-+/g, '-')         // Replace multiple - with single -
        .replace(/^-+/, '')             // Trim - from start of text
        .replace(/-+$/, '');            // Trim - from end of text
}

var getClosest = function (elem, selector) {
    for (; elem && elem !== document; elem = elem.parentNode) {
        if (elem.matches(selector)) return elem;
    }
    return null;
};

function getMailContent(id) {
    return new Promise((resolve, reject) => {
        var xhr = new XMLHttpRequest();
        xhr.open("GET", "https://mail.google.com/mail/u/0/?ui=2&view=pt&search=inbox&th=" + id);

        xhr.onload = function () {
            if (xhr.status < 300) {
                var responseText = xhr.responseText;
                responseText = responseText.replace("window.print()", "");

                resolve(responseText);
            }
            else {
                reject(xhr);
            }
        }

        xhr.send();
    });
}

function convertToPdf(id) {
    var search = document.location.hash.substr(1).split('/')[0];

    var iframe = Object.assign(document.createElement('iframe'), {
        width: 300,
        height: 300,
        style: `
        position: absolute;
        z-index: 10;
        left: 0;
        top: 0;
        background-color: #fff;
        visibility: hidden;
        pointer-events: none;
      `,
        src: '//mail.google.com/mail/u/0/?ui=2&view=pt&search=inbox&th=' + id + '&cm=save-as-pdf-jspdf'
    });

    document.body.appendChild(iframe);
}

function getEmailListItemTitle(row) {
    return row.querySelector(".bog").innerText;
}

function getEmailListItemId(row) {
    return row.querySelector("[data-legacy-thread-id]")
        .getAttribute("data-legacy-thread-id");
}

function convertEmail(emailTableRow) {
    var emailId = getEmailListItemId(emailTableRow);

    convertToPdf(emailId);
}

function onPdfButtonClick() {
    var selectedEmails = getSelectedEmails();

    if (selectedEmails.length == 0) {
        swal("", "Please select at least an email you want to Convert to PDF.");
    }

    for (var emailCheckbox of selectedEmails) {
        var emailTableRow = getClosest(emailCheckbox, "tr");
        convertEmail(emailTableRow);
    }
}

function tryGetGmailToolbar() {
    return document.querySelector(".G-tF");
}

function tryGetGmailToolbarButton(toolbar) {
    return toolbar.querySelector(".G-Ni.J-J5-Ji");
}

function createGmailToolbarButton(toolbar, iconUrl, tooltip) {
    var buttonContainer = document.createElement("div");
    buttonContainer.className = "G-Ni J-J5-Ji";
    toolbar.appendChild(buttonContainer);

    var buttonContent = document.createElement("div");
    buttonContent.className = "T-I J-J5-Ji nu T-I-ax7 L3";
    buttonContent.setAttribute("data-tooltip", tooltip);
    buttonContent.style.cssText = "user-select: none;";
    buttonContainer.appendChild(buttonContent);

    var iconContainer = document.createElement("div");
    iconContainer.className = "asa";
    buttonContent.appendChild(iconContainer);

    var buttonIcon = document.createElement("div");
    buttonIcon.className = "asf T-I-J3 J-J5-Ji";
    buttonIcon.style.cssText = "background-image: url(" + iconUrl + ")";
    iconContainer.appendChild(buttonIcon);



    return buttonContainer;
}

function getSelectedEmails() {
    return document.querySelectorAll("td div[role='checkbox'][aria-checked='true']");
}

var appendIconTimer = setInterval(function () {
    var toolbar = tryGetGmailToolbar();


    if (toolbar && tryGetGmailToolbarButton(toolbar)) {
        var icon = chrome.runtime.getURL("/images/icon-pdf.png");
        var btnToPdf = createGmailToolbarButton(toolbar, icon, "Convert to PDF");

        btnToPdf.addEventListener("click", onPdfButtonClick);

        clearInterval(appendIconTimer);
    }

}, 1000)
