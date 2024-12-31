document.getElementById('save').addEventListener('click', () => {
    const serverUrl = document.getElementById('serverUrl').value;
    const apiKey = document.getElementById('apiKey').value;

    chrome.storage.sync.set({ serverUrl, apiKey }, () => {
        console.log('Options saved');
    });
});

document.addEventListener('DOMContentLoaded', () => {
    chrome.storage.sync.get('serverUrl', (data) => {
        if (data.serverUrl) {
            document.getElementById('serverUrl').value = data.serverUrl;
        }
    });
});


