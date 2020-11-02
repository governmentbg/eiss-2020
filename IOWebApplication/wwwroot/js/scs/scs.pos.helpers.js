(function (root, factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        define(['scs'], factory);
    }
    else if (typeof module !== 'undefined' && module.exports) {
        module.exports = factory(require('scs'));
    }
    else {
        factory(root.SCS);
    }
}(this, function (SCS, undefined) {
    "use strict";
    SCS.posGetPort = function () {
        return SCS.invoke('pos/getPort', {});
    };
    SCS.posSetPort = function (port) {
        return SCS.invoke('pos/setPort', { "port": port });
    };
    SCS.posGetAvailablePorts = function () {
        return SCS.invoke('pos/getAvailablePorts', {});
    };

    SCS.posSendPayment = function (amount) {
        return SCS.invoke('pos/sendPayment', { "amount": amount });
    };
    SCS.posSendPaymentPort = function (portName, amount) {
        return SCS.invoke('pos/sendPayment', { "portName": portName, "amount": amount });
    };
}));
