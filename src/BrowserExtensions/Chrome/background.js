chrome.action.onClicked.addListener((tab) => {
    if (tab.url) {
        chrome.storage.sync.get(['serverUrl', 'apiKey'], (data) => {
            fetch(data.serverUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'BREEF-API-KEY': data.apiKey
                },
                body: JSON.stringify({ url: tab.url })
            })
                .then(response => response.json())
                .then(data => {
                    console.log('Success:', data);
                    chrome.notifications.create({
                        type: 'basic',
                        iconUrl: 'Images/breef-48.png',
                        title: 'Request Successful',
                        message: 'The URL was sent successfully.'
                    });
                })
                .catch((error) => {
                    console.error('Error:', error);
                    chrome.notifications.create({
                        type: 'basic',
                        iconUrl: 'Images/breef-48.png',
                        title: 'Request Failed',
                        message: 'There was an error sending the URL.'
                    });
                });
        });
    }
});
