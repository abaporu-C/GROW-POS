//var applicationServerPublicKey = '';
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
                    var p256dh = base64Encode(subscription.getKey('p256dh'));
                    var auth = base64Encode(subscription.getKey('auth'));

                    $('#PushEndpoint').val(subscription.endpoint);
                    $('#PushP256DH').val(p256dh);
                    $('#PushAuth').val(auth);
                    $('#btnUnsubscribe').prop("disabled", false);
                    $('#btnSubscribe').prop("disabled", true);
                    errorHandler('Currently subscribed to push notifications');
                } else {
                    console.log('NOT yet subscribed to push notifications');
                    $('#btnUnsubscribe').prop("disabled", true);
                    $('#btnSubscribe').prop("disabled", false);
                    //subscribe();
                }
            })
            .catch(function (err) {
                errorHandler('[req.pushManager.getSubscription] Unable to get subscription details.', err);
            });
    });
}


function unsubscribe() {
    navigator.serviceWorker.ready.then(function (reg) {
        reg.pushManager.getSubscription().then(function (subscription) {
            subscription.unsubscribe().then(function (successful) {
                // You've successfully unsubscribed
                errorHandler('You have Unsubscribed!');
                $('#PushEndpoint').val("");
                $('#PushP256DH').val("");
                $('#PushAuth').val("");
                $('#btnUnsubscribe').prop("disabled", true);
                $('#btnSubscribe').prop("disabled", false);
            }).catch(function (e) {
                // Unsubscribing failed
                errorHandler('[subscribe] Cannot unsubscribe to push', e);
            })
        })
    });
}

function subscribe() {
    navigator.serviceWorker.ready.then(function (reg) {
        var subscribeParams = { userVisibleOnly: true };

        //Setting the public key of our VAPID key pair.
        var applicationServerKey = urlB64ToUint8Array(applicationServerPublicKey);
        subscribeParams.applicationServerKey = applicationServerKey;

        reg.pushManager.subscribe(subscribeParams)
            .then(function (subscription) {
                isSubscribed = true;

                var p256dh = base64Encode(subscription.getKey('p256dh'));
                var auth = base64Encode(subscription.getKey('auth'));

                console.log(subscription);

                $('#PushEndpoint').val(subscription.endpoint);
                $('#PushP256DH').val(p256dh);
                $('#PushAuth').val(auth);
                $('#btnUnsubscribe').prop("disabled", false);
                $('#btnSubscribe').prop("disabled", true);
                $('#CreateForm').submit();
            })
            .catch(function (e) {
                errorHandler('[subscribe] Unable to subscribe to push', e);
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

function urlB64ToUint8Array(base64String) {
    var padding = '='.repeat((4 - base64String.length % 4) % 4);
    var base64 = (base64String + padding)
        .replace(/\-/g, '+')
        .replace(/_/g, '/');

    var rawData = window.atob(base64);
    var outputArray = new Uint8Array(rawData.length);

    for (var i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

function base64Encode(arrayBuffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(arrayBuffer)));
}
