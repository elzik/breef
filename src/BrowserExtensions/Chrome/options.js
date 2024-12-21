document.getElementById('save').addEventListener('click', () => {
    const serverUrl = document.getElementById('serverUrl').value;
    chrome.storage.sync.set({ serverUrl }, () => {
        console.log('Server URL saved:', serverUrl);
    });
});

document.addEventListener('DOMContentLoaded', () => {
    chrome.storage.sync.get('serverUrl', (data) => {
        if (data.serverUrl) {
            document.getElementById('serverUrl').value = data.serverUrl;
        }
    });
});
