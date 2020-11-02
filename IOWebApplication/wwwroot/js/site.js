$(function () {
    $('#messageContainer').delay(8000).fadeOut(2000);
    setInterval(CheckCertificate, 60000);
});

function JsonBGdate(value) {
    if (!value) {
        return '';
    }

    return moment(value).format("DD.MM.YYYY");
}
function JsonBGdateTime(value) {
    if (!value) {
        return '';
    }

    return moment(value).format("DD.MM.YYYY HH:mm");
}
//Преобразува handlebars template, който е съдържание в контейнер с подадено име
function TemplateToHtml(countainer, data) {
    var source = $(countainer).html();

    return HandlebarsToHtml(source, data);
}

//Преобразува handlebars template, 
function HandlebarsToHtml(hbTemplate, data) {
    var template = Handlebars.compile(hbTemplate);

    return template(data);
}

Handlebars.registerHelper('eachData', function (context, options) {
    var fn = options.fn, inverse = options.inverse, ctx;
    var ret = "";

    if (context && context.length > 0) {
        for (var i = 0, j = context.length; i < j; i++) {
            ctx = Object.create(context[i]);
            ctx.index = i;
            ret = ret + fn(ctx);
        }
    } else {
        ret = inverse(this);
    }
    return ret;
});

Handlebars.registerHelper("math", function (lvalue, operator, rvalue, options) {
    lvalue = parseFloat(lvalue);
    rvalue = parseFloat(rvalue);

    return {
        "+": lvalue + rvalue
    }[operator];
});

Handlebars.registerHelper("date", function (date) {
    let dateValue = date;
    return moment(dateValue).format("DD.MM.YYYY");
});

Handlebars.registerHelper("dateTime", function (date) {
    let dateValue = date;
    return moment(dateValue).format("DD.MM.YYYY HH:mm:ss");
});

Handlebars.registerHelper("dateTimeMin", function (date) {
    let dateValue = date;
    return moment(dateValue).format("DD.MM.YYYY HH:mm");
});

Handlebars.registerHelper('numberFormat', function (value, options) {
    if (isNaN(value)) {
        return "";
    }
    // Helper parameters
    var dl = options.hash['decimalLength'] || 2;
    var ts = options.hash['thousandsSep'] || ',';
    var ds = options.hash['decimalSep'] || '.';

    // Parse to float
    var valueFloat = parseFloat(value);

    // The regex
    var re = '\\d(?=(\\d{3})+' + (dl > 0 ? '\\D' : '$') + ')';

    // Formats the number with the decimals
    var num = valueFloat.toFixed(Math.max(0, ~~dl));

    // Returns the formatted number
    return (ds ? num.replace('.', ds) : num).replace(new RegExp(re, 'g'), '$&' + ts);
});

Handlebars.registerHelper("xif", function (expression, options) {
    return Handlebars.helpers["x"].apply(this, [expression, options]) ? options.fn(this) : options.inverse(this);
});

function requestGET_Json(url, data, callback) {
    $.ajax({
        type: 'GET',
        async: true,
        cache: false,
        contentType: "application/json;charset=utf-8",
        dataType: 'json',
        url: url,
        data: data,
        success: function (data) {
            if (callback) {
                callback(data);
            }
        }
    });
}

function postContent(url, data, callback) {
    $.ajax({
        type: 'POST',
        async: true,
        cache: false,
        //contentType: "application/json;charset=utf-8",
        //dataType: 'json',
        url: url,
        data: data,
        success: function (data) {
            if (callback) {
                callback(data);
            }
        }
    });
}

//Зарежда съдържанието на резултата от PartialView в div-елемент
function requestContent(url, data, callback) {

    $.ajax({
        type: 'GET',
        //async: true,
        cache: false,
        url: url,
        data: data,
        success: function (data) {
            callback(data);
        }
    });
}
//
function requestContentOk(url, data, callback) {

    $.ajax({
        type: 'GET',
        //async: true,
        cache: false,
        url: url,
        data: data,
        success: function (data) {
            if (data === 'ok') {
                callback();
            }
        }
    });
}
function fillCombo(items, combo, selected) {
    var tmlp = '{{#each this}}<option value="{{value}}" {{#if selected}}selected="selected"{{/if}}>{{text}}</option>{{/each}}';
    $(combo).html(HandlebarsToHtml(tmlp, setSetSelected(items, selected)));
}
function requestCombo(url, data, combo, selected, callback) {
    requestGET_Json(url, data, function (items) {
        fillCombo(items, combo, selected);
        if (callback) {
            callback(combo);
        }
    });
}

function setSetSelected(items, selected) {
    if (items && (selected !== undefined)) {
        for (var i = 0; i < items.length; i++) {
            if (items[i].value === selected.toString()) {
                items[i].selected = true;
            }
        }
    }

    return items;
}

// Показва съобщения от JS
var messageHelper = (function () {
    function ShowMessage(message, kind, container) {
        if (!container) {
            container = '#messageContainer';
        }

        let messageElement = '<div class="row"><div class="col-sm-12"><div class="alert alert-' + kind + ' no-margin-bottom">' +
            '<button data-dismiss="alert" class="close">×</button>' + message + '</div></div></div>';
        $(container).html(messageElement);
        $(container).show();
        $(container).delay(10000).slideUp(1000);
    }

    function ShowErrorMessage(message, container) {
        ShowMessage(message, 'danger', container);
        setTimeout(function () {
            singleClickSubmitEnable();
        }, 500);

    }

    function ShowSuccessMessage(message, container) {
        ShowMessage(message, 'success', container);
    }

    function ShowWarning(message, container) {
        ShowMessage(message, 'warning', container);
        setTimeout(function () {
            singleClickSubmitEnable();
        }, 500);
    }

    return {
        ShowErrorMessage: ShowErrorMessage,
        ShowSuccessMessage: ShowSuccessMessage,
        ShowWarning: ShowWarning
    };
})();

$.widget('custom.autocomplete_custom', $.ui.autocomplete, {
    _renderMenu: function _renderMenu(ul, items) {
        let that = this;
        let category = '';
        $.each(items, function (index, item) {
            if (item.category !== category) {
                ul.append('<li class="ui-autocomplete-category ui-state-disabled" aria-label="' + item.category + '">' + item.category + '</li>');
                category = item.category;
            }
            that._renderItemData(ul, item);
        });
    }
});

function initDynamicForms(addCallback) {
    $('div.dynamic-form').each(function (i, form) {
        $(form).on('click', 'a.add-item', function () {
            let addItem = this;
            requestContent($(this).data('url'), { index: $(this).data('index') }, function (html) {
                let index = parseInt($(addItem).data('index')) + 1;
                $(addItem).data('index', index);
                let container = $(document.getElementById($(addItem).data('container')));
                $(html).hide().appendTo(container).slideDown();
                if (addCallback) {
                    addCallback();
                }
            });
            return false;
        });
        $(form).on('click', 'a.remove-item', function () {
            let removeLink = this;
            if ($(removeLink).data('alert')) {
                swalConfirm($(removeLink).data('alert'), function () {
                    $(removeLink).parents('.item-template:first').hide('normal').remove();
                    return false;
                });
            } else {
                $(this).parents('.item-template:first').hide('normal').remove();
                return false;
            }
        });
    });
}

function attachAjaxForm(form, completeCallback, errorCallback, beforeSendCallback) {
    var submitButton = $(form).find('.submit:first');
    $(form).ajaxForm({
        beforeSend: function () {
            $(submitButton).attr('disabled', 'disabled');
            //debugger;
            //console.log('attachAjaxForm submited:'+Date.now.toString())
            if (beforeSendCallback) {
                beforeSendCallback();
            }
        },
        error: function (data) {
            if (errorCallback) {
                completeCallback(data);
            }
        },
        complete: function (data) {
            $(submitButton).removeAttr('disabled');
            if (completeCallback) {
                completeCallback(data);
            }
        }
    });
}


// Презарежда DataTable 'dataTableID' при 'change' събитието на елемент 'elementID'
function reloadDataTableOnElementChange(elementID, dataTableID) {
    $(document).on('change', elementID, function () {
        $(dataTableID).DataTable().ajax.reload(null, false);
    });
}


// Презарежда DataTable 'dataTableID' при 'change' събитието на 'DateTimePicker' елемент 'elementID'
function reloadDataTableOnDateTimePickerChange(elementID, dataTableID) {
    $(elementID).on('dp.change', function (e) {
        $(dataTableID).DataTable().ajax.reload(null, false);
    });
}


// Рефреш на 'DataTables' таблица с ID='tableID'
function refreshTable(dataTableID) {
    $(dataTableID).DataTable().ajax.reload(null, true);
    return true;
}



// зарежда dropdown-и по зададени параметри и 'ActionUrl'
function loadDropDownData(parameters, actionUrl, changeElementName, calbackFunction, callBackPars) {
    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        url: actionUrl,
        data: parameters,
        dataType: "json",
        beforeSend: function () {
        },
        success: function (data) {
            var items = "";
            $.each(data, function (i, item) {
                if (item.value == null) {
                    items += "<option value>" + item.text + "</option>";
                }
                else {
                    items += "<option value='" + item.value + "'>" + item.text + "</option>";
                }
            });
            // Fill Dropdown list
            $(changeElementName).html(items);
            if (calbackFunction && typeof (calbackFunction) == "function") {
                if (callBackPars != "undefined") {
                    calbackFunction(callBackPars.refreshTable, callBackPars.refreshElement);
                }
                else {
                    calbackFunction();
                }

            }
        },
        error: function (result) {
            writeJavascriptExceptionToErrorLog("Error in loadDropDownData()", result.status + ' Type :' + result.statusText);
        }
    });

}

// Функции за извличане на стойността на 'CheckBox' елемент
function GetCheckBoxValue(checkBoxID) {
    var sel = 'input[name="' + checkBoxID + '"]';
    sel = sel.replace('#', '');
    return $(sel).prop('checked');
}
function swalSubmit(sender, text) {
    swal({
        title: 'Потвърди',
        text: text,
        icon: "warning",
        buttons: ["Отказ", "Потвърди"]
        //dangerMode: true
    })
        .then((result) => {
            if (result) {
                $(sender).parents('form:first').trigger('submit');
            } else {
                return false;
            }
        });
}
function swalConfirm(text, callback, cancelCallback) {
    swal({
        title: 'Потвърди',
        text: text,
        icon: "warning",
        buttons: ["Отказ", "Потвърди"]
        //dangerMode: true
    })
        .then((result) => {
            if (result) {
                callback();
            } else if (cancelCallback) {
                cancelCallback();
            } else {
                return false;
            }
        });
}
function swalConfirmClick(e, sender, text) {
    if ($(sender).data('confirmed') === true) {
        return true;
    }
    swal({
        title: 'Потвърди',
        text: text,
        icon: "warning",
        buttons: ["Отказ", "Потвърди"]
        //dangerMode: true
    })
        .then((result) => {
            if (result) {
                $(sender).data('confirmed', true);
                $(sender).trigger('click');
            } else {
                return false;
            }
        });

    e.preventDefault();
    return false;
}
// 
function getFormData($form) {
    var unindexed_array = $form.serializeArray();
    var indexed_array = {};

    $.map(unindexed_array, function (n, i) {
        indexed_array[n['name']] = n['value'];
    });

    return indexed_array;
}

$(document).ready(function () {
    var time_labels = $('.timeline .time-label');
    time_labels.each(function (index, time_label) {
        var target = time_label.dataset.target;
        if (!!target) {
            $(time_label).addClass('time-label--toggle');
        }
    });

    $(document).on('click', '.timeline .time-label--toggle', function (e) {
        e.preventDefault();
        let element = e.currentTarget;
        let parent = element.parentElement;
        let target = element.dataset.target;
        let sub_lists = $(parent).find('[data-for="' + target + '"]');
        sub_lists.toggleClass('hidden');
    });

    $(document).on('click', '.timeline li[data-for] .timeline-item .timeline-header, .timeline li[data-for] > .fa', function (e) {
        e.preventDefault();
        let element = e.currentTarget;
        let parent = element.parentElement;
        let timeline_to_toggle = $(parent).find('.timeline-body, .timeline-footer');
        timeline_to_toggle.toggleClass('hidden');
    });
});

function check_egn(egn_str) {
    egn_str = egn_str.replace(/\s+/, '');
    egn_str = egn_str.replace(/\s+/, '');
    let egn_len = egn_str.length;
    if (egn_len == 10) {
        let egn = parseInt(egn_str);
        if (isNaN(egn)) {
            alert('Грешен ЕГН! Моля използвайте само цифри!');
            return false;
        } else {
            let weights = new Array(2, 4, 8, 5, 10, 9, 7, 3, 6);

            let sum = 0;
            for (var i = 0; i < 9; i++) {
                sum += egn_str.charAt(i) * weights[i];
            }

            let new_value = sum % 11;

            if (new_value == 10) { new_value = 0; }

            if (egn_str.charAt(9) == new_value) {
                return true;

            } else {

                return false;
            }

        }
    } else {
        //alert('Грешен ЕГН! Моля въведете 10 цифри без интервали!');
        return false;
    }
}

function check_eik(eik_str) {
    eik_str = eik_str.replace(/\s+/, '');
    let eik_len = eik_str.length;
    if ((eik_len === 9) || (eik_len === 13)) {
        let eik = parseInt(eik_str);
        if (isNaN(eik)) {
            //alert('Грешен ЕИК номер! Моля използвайте само цифри!');
            return false;
        } else {
            let sum = 0;
            for (var i = 0; i < 8; i++) {
                sum += eik_str.charAt(i) * (i + 1);
            }
            let new_value = sum % 11;
            if (new_value == 10) {
                sum = 0;
                for (i = 0; i < 8; i++) {
                    sum += eik_str.charAt(i) * (i + 3);
                }
                new_value = sum % 11;
                if (new_value === 10) {
                    new_value = 0;
                }
            }

            if (new_value == eik_str.charAt(8)) {
                if (eik_len === 9) {
                    return true;
                    //alert('Въведеният ЕИК е валиден!');
                } else {
                    let sum = eik_str.charAt(8) * 2 + eik_str.charAt(9) * 7 + eik_str.charAt(10) * 3 + eik_str.charAt(11) * 5;
                    new_value = sum % 11;
                    if (new_value === 10) {
                        sum = eik_str.charAt(8) * 4 + eik_str.charAt(9) * 9 + eik_str.charAt(10) * 5 + eik_str.charAt(11) * 7;
                        new_value = sum % 11;
                        if (new_value === 10) {
                            new_value = 0;
                        }
                    }
                    if (new_value == eik_str.charAt(12)) {
                        return true;
                        //alert('Въведеният ЕИК е валиден!');
                    } else {
                        //alert('Въведеният ЕИК е невалиден!');
                        return false;
                    }
                }
            } else {
                //alert('Въведеният ЕИК е невалиден!');
                return false;
            }
        }
    } else {
        //alert('Грешен ЕИК');
        return false;
    }
}

function check_date(value) {
    var ok_array = [
        'DD.MM.YYYY',
        'DD.MM.YYYY HH:mm',
        'DD.MM.YYYY HH:mm:ss',
        'MM.YYYY'
    ].filter(function (type_date) {
        return moment(value, type_date, true).isValid();
    });

    return !!ok_array[0];
}

function log_ajax_err(jqXHR, textStatus, errorThrown) {
    console.log({ jqXHR: jqXHR });
    console.log({ textStatus: textStatus });
    console.log({ errorThrown: errorThrown });
}
function setComboSelectedOrFirst($combo, selected) {
    $combo.val(selected);
    if ($combo.prop("selectedIndex") < 0) {
        $combo.prop("selectedIndex", 0);
    }
}

$(document).on('change.decimal_after_selecting', '.form-control-decimal', function () {
    var val = $(this).val();

    if (val !== '' && val.trim() !== '') {
        if (val.indexOf(',') !== -1) {
            $(this).val(parseFloat(val.replace(',', '.')).toFixed(2).replace('.', ','));
        } else {
            $(this).val(parseFloat(val).toFixed(2));
        }
    }
});


function updateUserSetting(setting, value) {
    var url = rootDir + 'Account/UpdateUserSetting';
    postContent(url, { setting: setting, value: value });
}

function GetBirthDayFromEgn(uic) {
    var result = null;
    if (uic.length == 10 && uic.match(/\D/) == null) {
        var year = parseInt(uic.substring(0, 2));
        var month = parseInt(uic.substring(2, 4));
        var day = parseInt(uic.substring(4, 6));
        if (month >= 1 && month <= 12) {
            year += 1900;
        }
        else if (month >= 21 && month <= 32) {
            month -= 20;
            year += 1800;
        }
        else if (month >= 41 && month <= 52) {
            month -= 40;
            year += 2000;
        }

        result = new Date(year, month - 1, day);
        if (isNaN(result.getTime())) {
            result = null;
        }
    }
    return result;
}

function GetPersonMaturity(uic) {
    var result = "-1";
    var birthDate = GetBirthDayFromEgn(uic);
    if (birthDate != null) {
        var now = new Date();

        var a = moment([now.getFullYear(), now.getMonth(), now.getDate()]);
        var b = moment([birthDate.getFullYear(), birthDate.getMonth(), birthDate.getDate()]);
        var years = a.diff(b, 'years');
        if (years < 14) {
            result = '3';
        }
        else if (years >= 14 && years < 18) {
            result = '2';
        }
        else {
            result = '1';
        }
    }
    else {
    }
    return result;
}

function SubmitSingleClick(sender) {
    $(sender).attr('disabled', 'disabled').parents('form:first').trigger('submit');
}

function singleClickSubmitDisable(sender) {
    var disabled = $(sender).is(':disabled') || $(sender).attr('disabled');

    if (!disabled) {
        $(sender).attr('disabled', 'disabled');
        $(sender).parents('form:first').trigger('submit');
    }
}
function singleClickSubmitEnable() {
    $('.single-click-submit').removeAttr("disabled");
}

function CheckCertificate() {
    if (certNo && certNo !== "" && certCheckPath !== "") {
        $.ajax({
            url: certCheckPath,
            type: "GET",
            xhrFields: {
                withCredentials: true
            },
            crossDomain: true,
            success: function (data) {
                var currentCertNo = data["certno"];
                if (!currentCertNo || certNo.replace(/^0+/, '').toUpperCase() !== currentCertNo.replace(/^0+/, '').toUpperCase()) {
                    window.location.href = signOutUrl;
                }
            }
        });
    }
}

function mqInfoLoad() {
    $('a.mq--info').click(function () {
        let _url = rootDir + 'epep/MqInfo';
        let _integrationType = 2;
        if ($(this).data('it')) {
            _integrationType = $(this).data('it');
        }
        let _data = { integrationType: $(this).data('it'), integrationType: _integrationType, sourceType: $(this).data('st'), sourceId: $(this).data('si') };
        let _title = 'Заявки за интеграция към ' + $(this).text();
        requestContent(_url, _data, function (html) {
            ShowModalDialog(_title, html, true);
        });
    });
}

function printPdfFile(url){
    var opts = 'width=700,height=500,toolbar=0,menubar=0,location=1,status=1,scrollbars=1,resizable=1,left=0,top=0';
    var newWindow = window.open(url, 'name', opts);
    newWindow.print();
    return false;

}