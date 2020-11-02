(function (root, factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        define(['scs'], factory);
    }
    else if(typeof module !== 'undefined' && module.exports) {
        module.exports = factory(require('scs'));
    }
    else {
        factory(root.SCS);
    }
}(this, function (SCS, undefined) {
    "use strict";
    SCS.signFile = function () {
        return SCS.invoke('signer/signFile', {});
    };
    SCS.signXML = function (data) {
        return SCS.invoke('signer/sign', { "signatureType" : "xmldsig", "content" : SCS.Base64Encode(data)});
    };
    SCS.signEx = function (data, charset, newline) {
        return SCS.invoke('signer/sign', { "signatureType" : "signature", "charset" : charset, "newline" : newline, "content" : SCS.Base64Encode(data)});
    };
    SCS.signWin = function (data) {
        return SCS.invoke('signer/sign', { "signatureType" : "signature", "charset" : "windows-1251", "newline" : "crlf", "content" : SCS.Base64Encode(data)});
    };
    SCS.signDigest = function (data) {
        return SCS.invoke('signer/signDigest', { "signatureType" : "signature", "content" : SCS.Base64Encode(data)});
    }; 
    SCS.signDigestMultiple = function (data) {
        var temp = [], i;
        for (i = 0; i < data.length; i++) {
            temp.push(SCS.Base64Encode(data[i]));
        }
        return SCS.invoke('signer/signDigest', { "signatureType": "signature", "content": temp });
    };
    SCS.signDigestSID = function (data, sid) {
        return SCS.invoke('signer/signDigest', { "signatureType": "signature", "content": SCS.Base64Encode(data), "sid": sid });
    };
    SCS.clearSigner = function () {
        return SCS.invoke('signer/clearSigner', {});
    };	
}));
