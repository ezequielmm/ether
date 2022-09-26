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
    MetamaskSelectedAccount: function (success, error, gameobject) {
        success = UTF8ToString(success);
        if(error){ error = UTF8ToString(error); }
        if(gameobject != "MetaMask") { gameobject = UTF8ToString(gameobject); }

        window.ethereum.request({ method: 'eth_requestAccounts' })
        .then(function (accounts) {
            var account = accounts[0];
            unity.SendMessage(gameobject, success, account);
        })
        .catch(function (errorMsg) {
            if(error) {
                var json = JSON.stringify(errorMsg);
                unity.SendMessage(gameobject, error, json);
            }
        });
        
    },
    MetamaskPersonalSign: function(account, message, success, error, gameobject) {
        success = UTF8ToString(success);
        if(error){ error = UTF8ToString(error); }
        if(gameobject != "MetaMask") { gameobject = UTF8ToString(gameobject); }
        var from = UTF8ToString(account);
        var msg = UTF8ToString(message);
        
        var params = [msg, from];
        var method = 'personal_sign';

        ethereum.request({method: method, params: params})
        .then(function (result) {
            unity.SendMessage(gameobject, success, result);
        })
        .catch(function (errorMsg) {
            if(error) {
                var json = JSON.stringify(errorMsg);
                unity.SendMessage(gameobject, error, json);
            }
        });
    }
};
// Add all of the functions above to C#
autoAddDeps(plugin,'$deps');
mergeInto(LibraryManager.library, plugin);