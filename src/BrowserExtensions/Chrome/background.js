chrome.action.onClicked.addListener((tab) => {
    if (tab.url) {
        fetch('http://localhost:5079/breefs', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ url: tab.url })
        })
            .then(response => response.json())
            .then(data => console.log('Success:', data))
            .catch((error) => console.error('Error:', error));
    }
});
