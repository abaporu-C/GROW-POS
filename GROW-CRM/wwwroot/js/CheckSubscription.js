//The applicationServerPublicKey  should be supplied in ViewData
var serviceWorker = 'sw.js';
var isSubscribed = false;
var pushIsSupported = 'serviceWorker' in navigator && 'PushManager' in window;

if (pushIsSupported) {
    // We need the service worker registration to check for a subscription
    navigator.serviceWorker.ready.then(function (reg) {
        // Do we already have a push message subscription?
        reg.pushManager.getSubscription()
            .then(function (subscription) {
                isSubscribed = subscription;
                if (isSubscribed) {

                    errorHandler('Currently subscribed to push notifications');
                } else {
                    errorHandler('NOT yet subscribed to push notifications');
                }
            })
            .catch(function (err) {
                errorHandler('[req.pushManager.getSubscription] Unable to get subscription details.', err);
            });
    });
}

function errorHandler(message, e) {
    if (typeof e === 'undefined') {
        e = null;
    }

    console.error(message, e);
    $("#errorMessage").append('<li>' + message + '</li>').parent().show();
}
