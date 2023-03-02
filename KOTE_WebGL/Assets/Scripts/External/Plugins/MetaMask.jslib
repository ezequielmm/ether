// In Unity 2020, this must be ES5 Compatible
// The Unity-Javascript Interface
var plugin = {
    $deps:{},
    HasUnityInstance: function (){
        return (typeof unity !== 'undefined');
    },
    IsMetamaskInstalled: function () {
        // Check window.ethereum for data. If missing, metamask is not installed
        return (typeof window.ethereum !== 'undefined');
    },
    MetamaskSelectAccount: function (promiseId, returnMethod, gameobject) {
        promiseId = UTF8ToString(promiseId);
        returnMethod = UTF8ToString(returnMethod);
        if(gameobject != "MetaMask") { gameobject = UTF8ToString(gameobject); }

        window.ethereum.request({ method: 'eth_requestAccounts' })
        .then(function (accounts) {
            var json = JSON.stringify({
                "promiseId": promiseId,
                "accountSelected": accounts[0]
            });
            unity.SendMessage(gameobject, returnMethod, json);
        })
        .catch(function (errorMsg) {
            if(error) {
                var json = JSON.stringify({
                    "promiseId": promiseId,
                    "error": errorMsg
                });
                unity.SendMessage(gameobject, returnMethod, json);
            }
        });
    },
    MetamaskPersonalSign: function(promiseId, account, message, returnMethod, gameobject) {
        promiseId = UTF8ToString(promiseId);
        success = UTF8ToString(success);
        returnMethod = UTF8ToString(returnMethod);
        if(gameobject != "MetaMask") { gameobject = UTF8ToString(gameobject); }

        var from = UTF8ToString(account);
        var msg = UTF8ToString(message);
        
        var params = [msg, from];
        var method = 'personal_sign';

        ethereum.request({method: method, params: params})
        .then(function (result) {
            var json = JSON.stringify({
                "promiseId": promiseId,
                "signedMessage": result
            });
            unity.SendMessage(gameobject, returnMethod, json);
        })
        .catch(function (errorMsg) {
            if(error) {
                var json = JSON.stringify({
                    "promiseId": promiseId,
                    "error": errorMsg
                });
                unity.SendMessage(gameobject, returnMethod, json);
            }
        });
    }
};
// Add all of the functions above to C#
autoAddDeps(plugin,'$deps');
mergeInto(LibraryManager.library, plugin);