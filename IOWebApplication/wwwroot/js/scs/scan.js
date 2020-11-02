var scanner = (function () {
    var is_loaded = false;
    var is_initialized = false;
    var is_first_page = true;
    var is_scanning = false;
    var sources = [];
    var source;
    var interval;
    var pages = [];

    const load = function () {
        return new Promise(function (resolve, reject) {
            if (!is_loaded) {
                SCS.get(signToolsPath,
                    function (json) {
                        var srv = json.services;
                        for (var i = 0; i < srv.length; i++) {
                            if (srv[i].root == 'scan') {
                                return true;
                            }
                        }
                        return false;
                    })
                    .then(function (json) {
                        is_loaded = true;
                        resolve(is_loaded);
                    })
                    .then(null, function (err) {
                        var message = 'Неуспешно зареждане на скенер.';

                        if (err && err.message) {
                            message += ' Грешка: ' + err.message;
                        }

                        reject(message);
                    });
            } else {
                resolve(is_loaded);
            }
        })
    };

    const init = function () {
        return new Promise(function (resolve, reject) {
            if (!is_initialized) {
                SCS.scanInit()
                    .then(function (json) {
                        is_initialized = true;
                        resolve(is_initialized);
                    })
                    .then(null, function (err) {
                        var message = 'Неуспешно инициализиране на скенер.';

                        if (err && err.message) {
                            message += ' Грешка: ' + err.message;
                        }

                        reject(message);
                    });
            } else {
                resolve(is_initialized);
            }
        })
    };

    const release = function () {
        return new Promise(function (resolve, reject) {
            if (is_initialized) {
                SCS.scanRelease()
                    .then(function (json) {
                        is_initialized = false;
                        resolve(is_initialized);
                    })
                    .then(null, function (err) {
                        var message = 'Неуспешно освобождаване на скенер.';

                        if (err && err.message) {
                            message += ' Грешка: ' + err.message;
                        }

                        reject(message);
                    });
            } else {
                resolve(is_initialized);
            }
        })
    };

    const getSources = function (evenIfselected) {
        const getSourcesExecutor = function (resolve, reject) {
            if (sources.length === 0 || evenIfselected) {
                SCS.scanGetSources()
                    .then(function (json) {
                        sources = json.sources;
                        resolve(sources);
                    })
                    .then(null, function (err) {
                        let message = "Не са открити сканиращи устройства";

                        if (err && err.message) {
                            message += ' Грешка: ' + err.message;
                        }

                        reject(message);
                    });
            } else {
                resolve(sources);
            }
        }

        return new Promise(getSourcesExecutor);
    };

    const scanStart = function () {
        return new Promise(function (resolve, reject) {
            SCS.scanStart(source.name, is_first_page, is_first_page)
                .then(function (json) {
                    is_first_page = false;
                    is_scanning = true;
                    resolve(is_scanning);
                })
                .then(null, function (err) {
                    var message = 'Неуспешно стартиране на сканиране.';

                    if (err && err.message) {
                        message += ' Грешка: ' + err.message;
                    }

                    reject(message);
                });
        })
    };

    const scanComplete = function () {
        return new Promise(function (resolve, reject) {
            SCS.scanComplete()
                .then(function (data) {
                    resolve(data.scanComplete)
                })
                .then(null, function (err) {
                    var message = 'Неуспешна проверка за сканиране.';

                    if (err && err.message) {
                        message += ' Грешка: ' + err.message;
                    }

                    reject(message);
                });
        });
    };

    const getPage = function () {
        return new Promise(function (resolve, reject) {
            SCS.scanGetPage()
                .then(function (data) {
                    pages.push(data.pageData);
                    resolve(data.pageData);
                })
                .then(null, function (err) {
                    var message = 'Неуспешно получаване на страница.';

                    if (err && err.message) {
                        message += ' Грешка: ' + err.message;
                    }

                    reject(message);
                });
        });
    };

    const getFile = function () {
        return new Promise(function (resolve, reject) {
            init()
                .then((is_initialized) => SCS.scanGetDoc())
                .then(function (data) {
                    resolve(data.docData);
                })
                .then(null, function (err) {
                    var message = 'Неуспешно получаване на файл.';

                    if (err && err.message) {
                        message += ' Грешка: ' + err.message;
                    }

                    reject(message);
                });
        });
    };

    const checkIfReady = async function (showPreview, showError) {
        try {
            var isComplete = await scanComplete();

            if (isComplete) {
                clearInterval(interval);
                is_scanning = false;
                var pageData = await getPage();

                while (pageData) {
                    showPreview(pageData);
                    pageData = await getPage();
                }
            }
        } catch (error) {
            showError(error || 'Грешка при сканиране');
        }
    };

    const setSource = (index) => {
        source = sources[index];
    };

    const loadScanners = function () {
        return new Promise(function (resolve, reject) {
            load()
                .then((is_loaded) => init())
                .then((is_initialized) => getSources(false))
                .then((sources) => {
                    resolve(sources);
                })
                .catch((reason) => {
                    reject(reason);
                });
        })
    };

    const scan = function (showPreview, showError) {
        return new Promise(function (resolve, reject) {
            init()
                .then((is_initialized) => scanStart())
                .then((is_scanning) => {
                    interval = setInterval(checkIfReady, 3000, showPreview, showError);
                    resolve(is_scanning);
                })
                .catch((reason) => {
                    reject(reason);
                });
        })
    };

    const getIsinitialized = function () {
        return is_initialized;
    }

    return {
        loadScanners,
        release,
        setSource,
        scan,
        getFile,
        getIsinitialized
    }
}());