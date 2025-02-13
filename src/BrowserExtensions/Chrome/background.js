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
                        message: 'The summarised article was successfully created.',
                        buttons: [
                            { title: 'Open in Wallabag' }
                        ],
                        requireInteraction: true
                    }, (notificationId) => {
                        chrome.notifications.onButtonClicked.addListener((notifId, btnIdx) => {
                            if (notifId === notificationId && btnIdx === 0) {
                                chrome.tabs.create({ url: data.publishedUrl });
                            }
                        });
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
