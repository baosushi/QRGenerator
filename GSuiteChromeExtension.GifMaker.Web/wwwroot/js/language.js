var languages;

function getLanguageText(key) {
    return $("[data-language-key='" + key + "']").html() || key;
}

function loadLanguageList() {
    return $.ajax({
        url: "/texts/languages.json",
    });
}

function initLanguageTextbox() {
    var cbo = $("[data-language-box]");

    cbo.html("");
    var selectedLanguage = $("body").attr("lang");

    for (var language of languages) {
        var item = $("<option></option>");
        item.html(language.Name + " (" + language.EnglishName + ")");
        item.val(language.Code);

        if (language.Code == selectedLanguage) {
            item.attr("selected", "");
        }

        cbo.append(item);
    }

    cbo.change(onLanguageSelected)
}

function onLanguageSelected() {
    var selectedLangaugeCode = $(this).find(":selected").val();

    var url = window.location.pathname || "/";
    if (!url.startsWith("/")) {
        url = "/" + url;
    }

    if (url.startsWith("/hl-")) {
        var nextSplash = url.indexOf("/", 1);
        url = url.substr(nextSplash);
    }

    url = "/hl-" + selectedLangaugeCode + url;
    window.location.href = url;
}

$(function () {
    loadLanguageList()
        .done(response => {
            languages = response;
            initLanguageTextbox();
        });
});