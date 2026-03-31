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
    SCS.signFile = function (compress) {
        return SCS.invoke('signer/signFile', {"compress" : compress});
    };
    SCS.signXML = function (data) {
        return SCS.invoke('signer/sign', { "signatureType" : "xmldsig", "content" : SCS.Base64Encode(data)});
    };
    SCS.signXML = function (data, elementID) {
        return SCS.invoke('signer/sign', { "signatureType" : "xmldsig", "elementID" : elementID, "content" : SCS.Base64Encode(data)});
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
    SCS.signRaw = function (data) {
        return SCS.invoke('signer/sign', { "signatureType" : "raw", "content" : data});
    };
    SCS.signMultiple = function (data) {
        var temp = [], i;
        for (i = 0; i < data.length; i++) {
            temp.push(SCS.Base64Encode(data[i]));
        }
        return SCS.invoke('signer/sign', { "signatureType" : "signature", "content" : temp });
    };
    SCS.signDigestMultiple = function (data) {
        var temp = [], i;
        for (i = 0; i < data.length; i++) {
            temp.push(SCS.Base64Encode(data[i]));
        }
        return SCS.invoke('signer/signDigest', { "signatureType" : "signature", "content" : temp });
    };
    SCS.signDigestSID = function (data, sid) {
        return SCS.invoke('signer/signDigest', { "signatureType" : "signature", "content" : SCS.Base64Encode(data), "sid" : sid});
    };
    SCS.signDigestSID = function (data, sid, ispades) {
        return SCS.invoke('signer/signDigest', { "signatureType" : "signature", "content" : SCS.Base64Encode(data), "sid" : sid, "pades" : ispades});
    };
    SCS.signRawSID = function (data, sid) {
        return SCS.invoke('signer/sign', { "signatureType" : "raw", "content" : data, "sid" : sid});
    };
    SCS.signRawDigestSID = function (data, sid) {
        return SCS.invoke('signer/signDigest', { "signatureType" : "raw", "content" : SCS.Base64Encode(data), "sid" : sid});
    };
    SCS.signSID = function (data, sid) {
        return SCS.invoke('signer/sign', { "signatureType" : "signature", "content" : SCS.Base64Encode(data), "sid" : sid});
    };	
    SCS.selectSigner = function () {
        return SCS.invoke('signer/selectSigner', {});
    };
    SCS.selectSigner = function (sn, issuerCN) {
        return SCS.invoke('signer/selectSigner', { "sn" : sn, "issuerCN" : issuerCN });
    };
    SCS.clearSigner = function () {
        return SCS.invoke('signer/clearSigner', {});
    };	
}));
