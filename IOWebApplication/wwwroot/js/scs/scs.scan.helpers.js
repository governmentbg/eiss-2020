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
    SCS.scanInit = function () {
        return SCS.invoke('scan/init', {});
    };
    SCS.scanRelease = function () {
        return SCS.invoke('scan/release', {});
    };
    SCS.scanGetSources = function () {
        return SCS.invoke('scan/getSources', {});
    };
    SCS.scanSetSource = function (source) {
        return SCS.invoke('scan/setSource', {"sourceName" : source});
    };
    SCS.scanStart = function (source, clearPageBuffer, clearDocBuffer) {
        return SCS.invoke('scan/start', {"sourceName" : source, "clearPageBuffer" : clearPageBuffer, "clearDocBuffer" : clearDocBuffer});
    };
    SCS.scanComplete = function () {
        return SCS.invoke('scan/scanComplete', {});
    };
    SCS.scanGetPage = function () {
        return SCS.invoke('scan/getPage', {});
    };
    SCS.scanGetDoc = function () {
        return SCS.invoke('scan/getDoc', {});
    };
}));
