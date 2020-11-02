(function (root, factory) {
    "use strict";
    if (typeof define === 'function' && define.amd) {
        define(factory);
    }
    else if(typeof module !== 'undefined' && module.exports) {
        module.exports = factory();
    }
    else {
        root.SCS = factory();
    }
}(this, function (undefined) {
    "use strict";
    var Base64={_keyStr:"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=",encode:function(e){var t="";var n,r,i,s,o,u,a;var f=0;e=Base64._utf8_encode(e);while(f<e.length){n=e.charCodeAt(f++);r=e.charCodeAt(f++);i=e.charCodeAt(f++);s=n>>2;o=(n&3)<<4|r>>4;u=(r&15)<<2|i>>6;a=i&63;if(isNaN(r)){u=a=64}else if(isNaN(i)){a=64}t=t+this._keyStr.charAt(s)+this._keyStr.charAt(o)+this._keyStr.charAt(u)+this._keyStr.charAt(a)}return t},decode:function(e){var t="";var n,r,i;var s,o,u,a;var f=0;e=e.replace(/[^A-Za-z0-9\+\/\=]/g,"");while(f<e.length){s=this._keyStr.indexOf(e.charAt(f++));o=this._keyStr.indexOf(e.charAt(f++));u=this._keyStr.indexOf(e.charAt(f++));a=this._keyStr.indexOf(e.charAt(f++));n=s<<2|o>>4;r=(o&15)<<4|u>>2;i=(u&3)<<6|a;t=t+String.fromCharCode(n);if(u!=64){t=t+String.fromCharCode(r)}if(a!=64){t=t+String.fromCharCode(i)}}t=Base64._utf8_decode(t);return t},_utf8_encode:function(e){e=e.replace(/\r\n/g,"\n");var t="";for(var n=0;n<e.length;n++){var r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r)}else if(r>127&&r<2048){t+=String.fromCharCode(r>>6|192);t+=String.fromCharCode(r&63|128)}else{t+=String.fromCharCode(r>>12|224);t+=String.fromCharCode(r>>6&63|128);t+=String.fromCharCode(r&63|128)}}return t},_utf8_decode:function(e){var t="";var n=0;var c2,c3;var r=c2=c3=0;while(n<e.length){r=e.charCodeAt(n);if(r<128){t+=String.fromCharCode(r);n++}else if(r>191&&r<224){c2=e.charCodeAt(n+1);t+=String.fromCharCode((r&31)<<6|c2&63);n+=2}else{c2=e.charCodeAt(n+1);c3=e.charCodeAt(n+2);t+=String.fromCharCode((r&15)<<12|(c2&63)<<6|c3&63);n+=3}}return t}}
    var request = function (url, data, method) {
            if (method === undefined) {
                method = data === undefined ? 'GET' : 'POST';
            }
            switch (method) {
                case 'GET':
                case 'HEAD':
                    return fetch(url, {
                        method : method
                    });
                default:
                    return fetch(url, {
                        method: method,
                        cache: "no-store",
                        body: JSON.stringify(data)
                    });
            }
        },
        discover = function (features) {
            return new Promise(function (resolve, reject) {
                var urls = [
                        "http://127.0.0.1:8090",
                        "https://127.0.0.1:8089",
                        "http://127.0.0.1:23125",
                        "https://127.0.0.1:23124",
                        "http://127.0.0.1:53953",
                        "https://127.0.0.1:53952"
                        //"https://127.0.0.1:8081",
                        //"https://localhost:53952",
                        //"https://127.0.0.1:53952",
                        //"https://localhost:23124",
                        //"https://127.0.0.1:23124",
                        //"https://localhost:8089",
                        //"https://127.0.0.1:8089",
                        //"http://localhost:53951",
                        //"http://127.0.0.1:53951",
                        //"http://localhost:23123",
                        //"http://127.0.0.1:23123",
                        //"http://localhost:8088",
                        //"http://127.0.0.1:8088"
                    ],
                    failures = 0;
                urls.forEach(function (v, i) {
                    request(v + '/version')
                        .then(function (response) {
                            if (parseInt(response.status, 10) !== 200) {
                                throw new Error(response.statusText);
                            }
                            return response.json();
                        })
                        .then(function (json) {
                            if (json.version === undefined ||
                                json.httpMethods === undefined ||
                                json.contentTypes === undefined ||
                                json.signatureTypes === undefined ||
                                json.selectorAvailable === undefined ||
                                json.hashAlgorithms === undefined
                            ) {
                                throw new Error("Invalid SCS version response");
                            }
                            if (features) {
                                var i, tmp1, tmp2;
                                if (typeof features === 'function') {
                                    if (!features(json)) {
                                        throw new Error("Did not satisfy features");
                                    }
                                } else {
                                    for (i in features) {
                                        if (features.hasOwnProperty(i)) {
                                            if (!json[i]) {
                                                throw new Error("Missing required key in version response");
                                            }
                                            if (typeof json[i] === "boolean" && json[i] !== features[i]) {
                                                throw new Error("Invalid required boolean value");
                                            }
                                            if (typeof json[i] === "string") {
                                                tmp1 = json[i].split(',').map(function (v) { return v.trim(); });
                                                tmp2 = features[i].split(",").map(function (v) { return v.trim(); });
                                                tmp2.forEach(function (v) {
                                                    if (tmp1.indexOf(v) === -1) {
                                                        throw new Error("Missing required array item");
                                                    }
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            return resolve({ url : v, version : json });
                        })
                        .then(null, function (ex) {
                            if (++ failures >= urls.length) {
                                reject(new Error("No compatible SCS found"));
                            }
                        });
                });
            });
        },
        invoke = function (method, data) {
            return (current ? current : get()).then(function (scs) {
                return request(scs.url + '/' + method, data)
                    //.then(null, function (error) {
                    //    //try again
                    //    return get().then(function (scs) {
                    //        return request(scs.url + '/' + method, data);
                    //    });
                    //})
                    .then(function (response) {
                        if (parseInt(response.status, 10) !== 200) {
                            throw new Error(response.statusText);
                        }
                        return response;
                    })
                    .then(function (response) {
                        return response.json();
                    })
                    .then(function (json) {
                        if (!json.status || json.status.toLowerCase() !== 'ok') {
                            var err = Error(json.reasonText || "Operation failed");
                            err.response = json;
                            throw err;
                        }
                        return json;
                    });
            });
        },
        get = function (url, features, retries, timeout) {
            url = url || jurl || signToolsPath;
            jurl = url;
            retries = retries || 25;
            timeout = timeout || 5000;
            var cntr = 0;
            return current = new Promise(function (resolve, reject) {
                var check = function () {
                    discover(features)
                        .then(function (json) {
                            resolve(json);
                        })
                        .then(null, function () {
                            if (cntr === 0) {
                                location.href = url;
                            }
                            if (++ cntr >= retries) {
                                reject('Could not discover service');
                            } else {
                                setTimeout(check, timeout);
                            }
                        });
                };
                check();
            });
        },
        current = null;
        var jurl = null;
    return {
        version : function () {
            return (current ? current : get()).then(function (scs) {
                return scs.version;
            });
        },
        sign : function (data) {
            if (typeof data === "string") {
                data = {
                    content : Base64.encode(data)
                };
            }
            return invoke('sign', data);
        },
        Base64Encode : function (data) {
            return Base64.encode(data);
        },
        Base64Decode : function (data) {
            return Base64.decode(data);
        },
        invoke : invoke,
        get : get
    };
}));
