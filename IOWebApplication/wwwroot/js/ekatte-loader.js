var ekatteUrls = {
    ekatteSearch: rootDir + 'Ajax/SearchEkatte?area=&query=',
    ekatteEisppSearch: rootDir + 'Ajax/SearchEkatteEispp?area=&query=',
    ekatteGet: rootDir + 'Ajax/GetEkatte?id=',
    streetSearch: rootDir + 'Ajax/SearchStreet?area=&query=',
    streetGet: rootDir + 'Ajax/GetStreet?street_code=',
    eisppEkatteGet: rootDir + 'Ajax/GetEkatteByEisppCode?eisppCode=',
    eisppEkatteGetCategory: rootDir + 'Ajax/GetEkatteByEisppCodeCategory?eisppCode='
    
};

function initEkatteDDL() {
    $('.address--control').each(function (i, e) {
        let ekatteControl = $(e).find('.ekatte-control')[0];
        if ($(ekatteControl).data('isloaded') === 'true') {
            return;
        }
        $(ekatteControl).data('isloaded', 'true');
        let input_h = $(ekatteControl).parents('.address--control:first').find('input[id$="CityCode"]:first');
        input_h.val($(ekatteControl).val());
        $(e).find('input[type="radio"][id$="AddressTypeId"]').change(function () {
            $(this).parents('.item-template:first').find('legend.address-legend:first span:first').text('Опса');
        });

        $(ekatteControl).change(function () {
            let input = this;
            if (input.value) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = input.value;
            }
            var addressContainer1 = $(ekatteControl).parents('.address--control:first');
            $(addressContainer1).find('input[id$="StreetCode_street"]').val("");
            $(addressContainer1).find('input[id$="StreetCode"]:first').val("");
        });

        var ekatteVal = $(e).find('.ekatte-val').val();

        if (ekatteVal) {
            let addressContainer = $(ekatteControl).parents('.address--control:first');
            let streetControl = $(addressContainer).find('input[id$="StreetCode_street"]');
            let streetVal = $(addressContainer).find('input[id$="StreetCode"]:first').val();

            if (streetVal) {
                loadStreet(streetControl, ekatteVal, streetVal);
            }
            let resAreaControl = $(addressContainer).find('input[id$="ResidentionAreaCode_street"]');
            let resAreaVal = $(addressContainer).find('input[id$="ResidentionAreaCode"]:first').val();


            if (resAreaVal) {
                loadStreet(resAreaControl, ekatteVal, resAreaVal);
            }
        }
    });
    initStreet();
}

function initEkatte() {
    $('.address--control').each(function (i, e) {
        let ekatteControl = $(e).find('.ekatte-control')[0];
        if ($(ekatteControl).data('isloaded') === 'true') {
            return;
        }
        $(ekatteControl).data('isloaded', 'true');

        $(e).find('select[id$="_CountryCode"]').change(function () {
            if ($(this).val() === 'BG') {
                $(e).find('.row.bg-address').show();
                $(e).find('.row.foreign-address').hide();
            } else {
                $(e).find('.row.bg-address').hide();
                $(e).find('.row.foreign-address').show();
            }
        }).trigger('change');

        $.widget('custom.autocomplete_custom', $.ui.autocomplete, {
            _renderMenu: function _renderMenu(ul, items) {
                let that = this;
                var category = '';
                $.each(items, function (index, item) {
                    if (item.category !== category) {
                        ul.append('<li class="ui-autocomplete-category ui-state-disabled" aria-label="' + item.category + '">' + item.category + '</li>');
                        category = item.category;
                    }
                    that._renderItemData(ul, item);
                });
            }
        });

        $(ekatteControl).autocomplete_custom({
            minLength: 3,
            delay: 100,
            classes: {
                'ui-autocomplete': 'autocomplete_custom'
            },
            source: function source(request, response) {
                $.get(ekatteUrls.ekatteSearch + encodeURIComponent(request.term)).done(function (success) {
                    //location_storage[request.term] = success.data;
                    return response(success.data);
                }).fail(function (errors) {
                    console.log(errors);
                });
            },
            select: function select(event, ui) {
                ui.item.value = ui.item.label + ', ' + ui.item.category;
                var input_hidden = event.target.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = ui.item.id;
            }
        }).change(function () {
            var input = this;
            if (!input.value || input.value < 2) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = '';
            }
        }).blur(function () {
            var input = this;
            if (!input.value || input.value < 2) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = '';
            }
        });

        var ekatteVal = $(e).find('.ekatte-val').val();
        if (ekatteVal) {
            loadEkatte(ekatteControl, ekatteVal, function () {
                let addressContainer = $(ekatteControl).parents('.address--control:first');
                let streetControl = $(addressContainer).find('input[id$="StreetCode_street"]');
                let streetVal = $(addressContainer).find('input[id$="StreetCode"]:first').val();
                //debugger;
                if (streetVal) {
                    loadStreet(streetControl, ekatteVal, streetVal);
                }

                let resAreaControl = $(addressContainer).find('input[id$="ResidentionAreaCode_street"]');
                let resAreaVal = $(addressContainer).find('input[id$="ResidentionAreaCode"]:first').val();

                if (resAreaVal) {
                    loadStreet(resAreaControl, ekatteVal, resAreaVal);
                }
            });
        }
    });

    initStreet();
}
function loadEkatte(ekatteControl, ekatte, callback) {
    $.get(ekatteUrls.ekatteGet + ekatte)
        .done(function (data) {
            $(ekatteControl).val(data.label + ', ' + data.category);
            if (callback) {
                callback();
            }
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initStreet() {
    $('.street-container').each(function (i, e) {
        var streetControl = $(e).find('.street-control')[0];
        if ($(streetControl).data('isloaded') === 'true') {
            return;
        }
        $(streetControl).data('isloaded', 'true');

        $(streetControl).autocomplete({
            minLength: 3,
            delay: 100,
            appendTo: document.getElementById($(streetControl).attr('id') + "list"),//'body',
            source: function source(request, response) {
                var ekatte = $(streetControl).parents('.address--control:first').find('input.ekatte-val:first').val();
                var streetType = $(streetControl).parents('.street-container:first').data('streettype');
                //alert(streetType);
                $.get(ekatteUrls.streetSearch + encodeURIComponent(request.term) + '&ekatte=' + ekatte + '&streetType=' + streetType).done(function (success) {
                    return response(success);
                }).fail(function (errors) {
                    console.log(errors);
                });
            },
            select: function select(event, ui) {
                var input_hidden = event.target.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = ui.item.value;
                ui.item.value = ui.item.label;
            }, focus: function (event, ui) {
                $(streetControl).val(ui.item.label);
                return false;
            }
        }).change(function () {
            var input = this;
            if (!input.value || input.value < 2) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = '';
            }
        }).blur(function () {
            var input = this;
            if (!input.value || input.value < 2) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = '';
            }
        });


    });
}

function loadStreet(streetControl, ekatte, street) {
    $.get(ekatteUrls.streetGet + street + '&ekatte=' + ekatte)
        .done(function (data) {
            //debugger;
            $(streetControl).val(data.label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initEisppEkatte() {
    $('.eisppEkatte-container').each(function (i, e) {
        var _control = e;
        if ($(_control).data('isloaded') === 'true') {
            return;
        }
        $(_control).data('isloaded', 'true');
        $.get(ekatteUrls.eisppEkatteGet + $(_control).data('code'))
            .done(function (success) {
                $(_control).html(success.label);
            });
    });
}
function loadEkatteCrime(ekatteControl, ekatte, callback) {
    $.get(ekatteUrls.eisppEkatteGetCategory + ekatte)
        .done(function (data) {
            $(ekatteControl).val(data.label + ', ' + data.category);
            if (callback) {
                callback();
            }
        }).fail(function (errors) {
            console.log(errors);
        });
}
function initEisppEkatteCrime() {
    $('.crime-address-control').each(function (i, e) {
        let ekatteControl = $(e).find('.crime-ekatte-control')[0];
        if ($(ekatteControl).data('isloaded') === 'true') {
            return;
        }
        $(ekatteControl).data('isloaded', 'true');

        $(e).find('select[id$="_Country"]').change(function () {
            if ($(this).val() === '8805') {
                $(e).find('.bg-address').show();
                $(e).find('.foreign-address').hide();
            } else {
                $(e).find('.bg-address').hide();
                $(e).find('.foreign-address').show();
            }
        }).trigger('change');

        $.widget('custom.autocomplete_custom', $.ui.autocomplete, {
            _renderMenu: function _renderMenu(ul, items) {
                let that = this;
                var category = '';
                $.each(items, function (index, item) {
                    if (item.category !== category) {
                        ul.append('<li class="ui-autocomplete-category ui-state-disabled" aria-label="' + item.category + '">' + item.category + '</li>');
                        category = item.category;
                    }
                    that._renderItemData(ul, item);
                });
            }
        });

        $(ekatteControl).autocomplete_custom({
            minLength: 3,
            delay: 100,
            classes: {
                'ui-autocomplete': 'autocomplete_custom'
            },
            source: function source(request, response) {
                $.get(ekatteUrls.ekatteEisppSearch + encodeURIComponent(request.term)).done(function (success) {
                     return response(success.data);
                }).fail(function (errors) {
                    console.log(errors);
                });
            },
            select: function select(event, ui) {
                ui.item.value = ui.item.label + ', ' + ui.item.category;
                var input_hidden = event.target.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = ui.item.id;
            }
        }).change(function () {
            var input = this;
            if (!input.value || input.value < 2) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = '';
            }
        }).blur(function () {
            var input = this;
            if (!input.value || input.value < 2) {
                var input_hidden = input.parentElement.querySelector('input[type="hidden"]');
                input_hidden.value = '';
            }
        });

        var ekatteVal = $(e).find('.ekatte-val').val();
        if (ekatteVal) {
            loadEkatteCrime(ekatteControl, ekatteVal, function () {
                let addressContainer = $(ekatteControl).parents('.crime-address-control:first');
                //let resAreaControl = $(addressContainer).find('input[id$="ResidentionAreaCode_street"]');
                //let resAreaVal = $(addressContainer).find('input[id$="ResidentionAreaCode"]:first').val();

                //if (resAreaVal) {
                //    loadStreet(resAreaControl, ekatteVal, resAreaVal);
                //}
            });
        }
    });
}