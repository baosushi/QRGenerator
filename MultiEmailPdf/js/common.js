'use strict';

chrome.runtime.onMessage.addListener(async (request, sender, response) => {
    const tabId = sender.tab.id;
    if (request.method === 'image-to-data') {
        const img = new Image();
        img.crossOrigin = 'anonymous';
        img.onload = () => {
            const canvas = document.createElement('canvas');
            canvas.width = img.width;
            canvas.height = img.height;

            const ctx = canvas.getContext('2d');
            ctx.webkitImageSmoothingEnabled = true;
            ctx.mozImageSmoothingEnabled = true;
            ctx.imageSmoothingEnabled = true;
            ctx.imageSmoothingQuality = 'high';
            ctx.drawImage(img, 0, 0);
            response(canvas.toDataURL('image/png', 1.0));
        };
        img.onerror = () => response();
        img.src = request.src;

        return true;
    }

    else if (request.method === 'convert-to-pdf') {
        chrome.tabs.executeScript(tabId, {
            runAt: 'document_start',
            allFrames: false,
            file: '/js/jspdf.min.js',
            frameId: sender.frameId
        }, () => {
            chrome.tabs.executeScript(tabId, {
                runAt: 'document_start',
                allFrames: false,
                file: '/js/save.js',
                frameId: sender.frameId
            });
        });
    }
    else if (request.method === 'download') {
        const prefs = {
            format: '[title] - [date] [time]',
            saveAs: false,
            debug: false,
            fileName: request.fileName,
        };
        const time = new Date();
        const gmt = (new Date(time - time.getTimezoneOffset() * 60000))
            .toISOString().slice(2, 10).replace(/[^0-9]/g, '');

        const filename = prefs.fileName || prefs.format
            .replace('[title]', sender.tab.title)
            .replace('[simple-title]', sender.tab.title.replace(/\s-\s[^\s]+@.*$/, ''))
            .replace('[date]', time.toLocaleDateString().replace(/[:/]/g, '.'))
            .replace('[time]', time.toLocaleTimeString().replace(/[:/]/g, '.'))
            .replace('[gmt]', gmt)
            .replace(/[`~!@#$%^&*()_|+=?;:'",<>{}[\]\\/]/gi, '_')
            .replace('.pdf', '') + '.pdf';

        fetch(request.url)
            .then(res => res.blob())
            .then(blob => {
                const url = URL.createObjectURL(blob);
                chrome.downloads.download({
                    filename,
                    url,
                    saveAs: prefs.saveAs
                }, () => {
                    window.setTimeout(() => {
                        URL.revokeObjectURL(url);
                        chrome.tabs.executeScript(tabId, {
                            runAt: 'document_start',
                            allFrames: false,
                            code: `
                if (${prefs.debug} === false) {
                  try {
                    window.iframe.remove();
                  }
                  catch (e) {}
                }
                delete window.iframe;
                document.querySelector('[data-cmd=${request.cmd}]').dataset.working = false;
              `
                        });
                    });
                });
            });

        return false;
    }
});