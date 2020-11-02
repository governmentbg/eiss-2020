let autocompleteUrls = {
    lawUnitSearch: rootDir + 'LawUnit/SearchLawUnit?area=&query=',
    lawUnitGet: rootDir + 'LawUnit/GetLawUnit?id=',
    userSearch: rootDir + 'Account/SearchUser?query=',
    userGet: rootDir + 'Account/GetUser?id=',
    caseCodeSearch: rootDir + 'Ajax/Get_CaseCodeByLoadGroup?query=',
    caseCodeGet: rootDir + 'Ajax/Get_CaseCodeByLoadGroup?id=',
    caseCodeSearchModal: rootDir + 'Ajax/SearchCaseCode',
    caseSearch: rootDir + 'Ajax/Get_Case?query=',
    caseGet: rootDir + 'Ajax/Get_Case?caseId=',
    casePreview: rootDir + 'Case/CasePreview?id=',
    courtSearch: rootDir + 'Ajax/Get_Courts?query=',
    courtGet: rootDir + 'Ajax/Get_Courts?id=',
    balancePaymentSearch: rootDir + 'Money/SearchBalancePayment?query=',
    balancePaymentGet: rootDir + 'Money/GetPayment?id=',
    documentSearch: rootDir + 'Document/SearchDocument?query=',
    documentGet: rootDir + 'Document/GetDocumentById?id=',
    documentPreview: rootDir + 'Document/View?id=',
    eisppGetActualData: rootDir + 'Eispp/GetActualData',
    institutionSearch: rootDir + 'Ajax/Get_Institution?query=',
    institutionGet: rootDir + 'Ajax/Get_Institution?id=',
    caseloadAddActivitySearch: rootDir + 'Ajax/Get_CaseloadAddActivity?query=',
    caseloadAddActivityGet: rootDir + 'Ajax/Get_CaseloadAddActivity?id=',
    actLawBaseSearch: rootDir + 'Ajax/Search_ActLawBase?query=',
    actLawBaseGet: rootDir + 'Ajax/Get_ActLawBase?id=',
};


function initAutoCompleteControl(control, url, query, parentControl, paramFunc) {
    if ($(control).data('isloaded') === true) {
        return;
    }
    if (!query) {
        query = '';
    }
    let minLength = 3;
    if ($(control).data('minlength')) {
        minLength = $(control).data('minlength');
    }
    $(control).data('isloaded', true);
    $(control).autocomplete({
        minLength: minLength,
        delay: 155,
        appendTo: document.getElementById($(control).attr('id') + "list"),//'body',
        source: function source(request, response) {
            if (parentControl) {
                query += $(parentControl).val();
            }
            let params = '';
            if (paramFunc) {
                params += paramFunc();
            }
            $.get(url + encodeURIComponent(request.term) + query + params).done(function (success) {
                return response(success);
            }).fail(function (errors) {
                console.log(errors);
            });
        }
        , select: function select(event, ui) {
            //console.log(ui.item);
            let id = ui.item.value;
            ui.item.value = ui.item.label;
            let input_hidden = event.target.parentElement.querySelector('input[type="hidden"]');
            input_hidden.value = id;
            $(control).parent().find('span.description').text(ui.item.description);
            $(control).parent().trigger('change');
        }, focus: function (event, ui) {
            $(control).val(ui.item.label);
            return false;
        }
    }).change(function () {
        let input = this;
        if (!input.value || input.value < minLength) {
            let input_hidden = input.parentElement.querySelector('input[type="hidden"]');
            input_hidden.value = '0';
            $(control).parent().find('span.description').text('');
        }
    }).blur(function () {
        let input = this;
        if (!input.value || input.value < minLength) {
            let input_hidden = input.parentElement.querySelector('input[type="hidden"]');
            input_hidden.value = '0';
            $(control).parent().find('span.description').text('');
        }
    });
}

function initLawUnit() {
    $('.lawunit-container').each(function (i, e) {

        let lawUnitControl = $(e).find('.lawunit-control')[0];

        let lawUnitType = $(lawUnitControl).parents('.lawunit-container:first').data('lawunittype');
        let lawUnitTypes = $(lawUnitControl).parents('.lawunit-container:first').data('lawunittypes');
        let courtId = $(lawUnitControl).parents('.lawunit-container:first').data('courtid');
        let selectmode = $(lawUnitControl).parents('.lawunit-container:first').data('selectmode');

        initAutoCompleteControl(lawUnitControl, autocompleteUrls.lawUnitSearch, '&lawUnitType=' + lawUnitType + '&lawUnitTypes=' + lawUnitTypes + '&courtId=' + courtId + '&selectmode=' + selectmode);


        let lawUnitVal = $(e).find('.lawunit-val').val();
        if (lawUnitVal && lawUnitVal !== '0') {
            loadLawUnit(lawUnitControl, lawUnitVal);
        }
    });
}

function loadLawUnit(lawUnitControl, lawUnit) {
    $.get(autocompleteUrls.lawUnitGet + lawUnit)
        .done(function (data) {
            $(lawUnitControl).val(data.label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initCase() {
    $('.case-container').each(function (i, e) {

        let caseControl = $(e).find('.case-control')[0];

        initAutoCompleteControl(caseControl, autocompleteUrls.caseSearch, null, null, function () {
            let courtId = $(caseControl).parents('.case-container:first').data('court');
            return '&courtId=' + courtId;
        });


        let caseVal = $(e).find('.case-val').val();
        if (caseVal && caseVal !== '0') {
            loadCase(caseControl, caseVal);
        }
    });
}

function loadCase(caseControl, caseId) {
    $.get(autocompleteUrls.caseGet + caseId)
        .done(function (data) {
            $(caseControl).val(data[0].label);
            $(caseControl).parents('.case-container:first').trigger('change');
        }).fail(function (errors) {
            console.log(errors);
        });
}



function initUserAutoComplete() {
    $('.userautocomplete-container').each(function (i, e) {

        let userControl = $(e).find('.userautocomplete-control')[0];

        initAutoCompleteControl(userControl, autocompleteUrls.userSearch);

        let val = $(e).find('.userautocomplete-val').val();
        if (val && val !== '0') {
            loadUserAutoComplete(userControl, val);
        }
    });
}

function loadUserAutoComplete(autoControl, val) {
    $.get(autocompleteUrls.userGet + val)
        .done(function (data) {
            $(autoControl).val(data.label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initCaseCode() {
    $('.casecode-container').each(function (i, e) {

        let caseCodeControl = $(e).find('.casecode-control')[0];

        initAutoCompleteControl(caseCodeControl, autocompleteUrls.caseCodeSearch, undefined, undefined, function () {

            let caseTypeId = 0;
            if ($(caseCodeControl).parents('.casecode-container:first').data('casetype')) {
                caseTypeId = $(caseCodeControl).parents('.casecode-container:first').data('casetype');
            } else {
                let caseTypeContainer = $(caseCodeControl).parents('.casecode-container:first').data('casetypecontainer');
                caseTypeId = $(caseTypeContainer).val();
            }
            //debugger;
            return '&caseTypeId=' + caseTypeId;
        });

        let caseCodeVal = $(e).find('.casecode-val').val();
        if (caseCodeVal && caseCodeVal !== '0') {
            loadCaseCode(caseCodeControl, caseCodeVal);
            $(e).trigger('change');
        }
    });
}

function loadCaseCode(caseCodeControl, caseCode, emptyVal) {
    $.get(autocompleteUrls.caseCodeGet + caseCode)
        .done(function (data) {
            if (data.length > 0) {
                $(caseCodeControl).val(data[0].label);
                $(caseCodeControl).parent().find('input[type="hidden"]:first').val(data[0].value);
                $(caseCodeControl).parent().find('.description').text(data[0].description);
                $(caseCodeControl).parents('.casecode-container:first').trigger('change');
            } else {
                $(caseCodeControl).parent().find('input.ui-autocomplete-input').val('');
                if (emptyVal) {
                    $(caseCodeControl).parent().find('input[type="hidden"]:first').val(emptyVal);
                }
                $(caseCodeControl).parent().find('.description').text('');
            }
        }).fail(function (errors) {
            console.log(errors);
        });
}

function searchCaseCode(sender) {
    let caseType = $(sender).parents('.casecode-container:first').data('casetypeid');
    if (!caseType) {
        let caseTypeControl = $(sender).parents('.casecode-container:first').data('casetypecontainer');
        caseType = $(caseTypeControl).val();
    }
    let containerId = $(sender).parents('.casecode-container:first').data('container');
    let url = autocompleteUrls.caseCodeSearchModal;
    requestContent(url, { containerId: containerId, caseTypeId: caseType, callback: 'searchCaseCodeCallBack' }, function (html) {
        ShowModalDialog('Изберете шифър', html, true);
    });
}

function searchCaseCodeCallBack(containerId, caseCode) {
    let container = $('div[data-container="' + containerId + '"]')[0];
    let control = $(container).find('.casecode-control:first');
    loadCaseCode(control, caseCode);
    //console.log(container);
    $(container).trigger('change');
    HideModal();
}

function initBalancePayment() {
    $('.payment-container').each(function (i, e) {

        let paymentControl = $(e).find('.payment-control')[0];

        let moneyGroupId = $(paymentControl).parents('.payment-container:first').data('moneygroupid');

        initAutoCompleteControl(paymentControl, autocompleteUrls.balancePaymentSearch, '&moneyGroupId=' + moneyGroupId);

        let paymentVal = $(e).find('.payment-val').val();
        if (paymentVal && paymentVal !== '0') {
            loadPayment(paymentControl, paymentVal);
        }
    });
}

function loadPayment(paymentControl, payment) {
    $.get(autocompleteUrls.balancePaymentGet + payment)
        .done(function (data) {
            $(paymentControl).val(data.label);
        }).fail(function (errors) {
            console.log(errors);
        });
}


function initCourts() {
    $('.court-container').each(function (i, e) {

        let courtControl = $(e).find('.court-control')[0];

        initAutoCompleteControl(courtControl, autocompleteUrls.courtSearch);

        let courtVal = $(e).find('.court-val').val();
        if (courtVal && courtVal !== '0') {
            loadCourts(courtControl, courtVal);
        }
    });
}

function loadCourts(courtControl, val) {
    $.get(autocompleteUrls.courtGet + val)
        .done(function (data) {
            $(courtControl).val(data[0].label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initDocument() {
    $('.document-container').each(function (i, e) {

        let documentControl = $(e).find('.document-control')[0];

        initAutoCompleteControl(documentControl, autocompleteUrls.documentSearch, undefined, undefined, function () {
            if ($(e).data('courtId')) {
                return '&courtId=' + $(e).data('courtId');
            } else {
                return '';
            }
        });

        let documentVal = $(e).find('.document-val').val();
        if (documentVal && documentVal !== '0') {
            loadDocument(documentControl, documentVal);
        }
    });
}

function loadDocument(documentControl, document) {
    $.get(autocompleteUrls.documentGet + document)
        .done(function (data) {
            $(documentControl).val(data.label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function searchEisppNumber(sender) {
    let eisppNumber = $(sender).parents('.eisppnumber-container:first').find('.eisppnumber-control:first').val();
    let url = autocompleteUrls.eisppGetActualData;
    requestContent(url, { eisppNumber: eisppNumber }, function (html) {
        ShowModalDialog('Актуални данни по НП ' + eisppNumber, html, true);
    });
}

function showCasePreview(sender) {
    let caseId = $(sender).parents('.case-container:first').find('.case-val:first').val();
    if (caseId && caseId.length > 0) {
        let url = autocompleteUrls.casePreview + caseId;
        window.open(url, '_blank');
        //document.location.href = url;
    }
}


function showDocumentPreview(sender) {
    let docId = $(sender).parents('.document-container:first').find('.document-val:first').val();
    if (docId && docId.length > 0) {
        let url = autocompleteUrls.documentPreview + docId;
        window.open(url, '_blank');
        //document.location.href = url;
    }
}

function initInstitution() {
    $('.institution-container').each(function (i, e) {

        let institutionControl = $(e).find('.institution-control')[0];

        initAutoCompleteControl(institutionControl, autocompleteUrls.institutionSearch, undefined, undefined, function () {

            let institutionTypeId = 0;
            if ($(institutionControl).parents('.institution-container:first').data('institutiontype')) {
                institutionTypeId = $(institutionControl).parents('.institution-container:first').data('institutiontype');
            } else {
                let institutionTypeContainer = $(institutionControl).parents('.institution-container:first').data('institutiontypecontainer');
                institutionTypeId = $(institutionTypeContainer).val();
            }
            return '&institutionTypeId=' + institutionTypeId;
        });

        let institutionVal = $(e).find('.institution-val').val();
        if (institutionVal && institutionVal !== '0') {
            loadInstitution(institutionControl, institutionVal);
            $(e).trigger('change');
        }
    });
}

function loadInstitution(institutionControl, institution) {
    //console.log('loaded inst');
    $.get(autocompleteUrls.institutionGet + institution)
        .done(function (data) {
            if (data.length > 0) {
                //console.log(data);
                $(institutionControl).val(data[0].label);
                $(institutionControl).parent().find('input[type="hidden"]:first').val(data[0].value);
                //$(institutionControl).parent().find('.description').text(data[0].description);
                $(institutionControl).parents('.institution-container:first').trigger('change');
            } else {
                $(institutionControl).parent().find('input').val('');
                //$(institutionControl).parent().find('.description').text('');
            }
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initCaseLoadAddActivity() {
    $('.caseloadaddactivity-container').each(function (i, e) {

        let caseloadaddactivityControl = $(e).find('.caseloadaddactivity-control')[0];

        initAutoCompleteControl(caseloadaddactivityControl, autocompleteUrls.caseloadAddActivitySearch);

        let caseloadaddactivityVal = $(e).find('.caseloadaddactivity-val').val();
        if (caseloadaddactivityVal && caseloadaddactivityVal !== '0') {
            loadCaseLoadAddActivity(caseloadaddactivityControl, caseloadaddactivityVal);
        }
    });
}

function loadCaseLoadAddActivity(caseloadaddactivityControl, val) {
    $.get(autocompleteUrls.caseloadAddActivityGet + val)
        .done(function (data) {
            $(caseloadaddactivityControl).val(data[0].label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initEISPPTblElement() {
    $('.eispptblelement-container').each(function (i, e) {

        let eisppTblCode = $(e).data('tblcode');
        let eispptblelementControl = $(e).find('.eispptblelement-control')[0];

        initAutoCompleteControl(eispptblelementControl, rootDir + 'Ajax/Get_EISPPTblElement?eisppTblCode=' + eisppTblCode + '&query=');

        let eispptblelementVal = $(e).find('.eispptblelement-val').val();
        if (eispptblelementVal && eispptblelementVal !== '0') {
            loadEISPPTblElement(eispptblelementControl, eispptblelementVal);
        }
    });
}

function loadEISPPTblElement(eispptblelementControl, val) {
    let eisppTblCode = $(eispptblelementControl).parents('.eispptblelement-container:first').data('tblcode');
    $.get(rootDir + 'Ajax/Get_EISPPTblElement?eisppTblCode=' + eisppTblCode + '&id=' + val)
        .done(function (data) {
            $(eispptblelementControl).val(data[0].label);
        }).fail(function (errors) {
            console.log(errors);
        });
}

function initActLawBaseAutoComplete() {
    $('.actlawbaseautocomplete-container').each(function (i, e) {

        let actLawBaseControl = $(e).find('.actlawbaseautocomplete-control')[0];

        initAutoCompleteControl(actLawBaseControl, autocompleteUrls.actLawBaseSearch);

        let val = $(e).find('.actlawbaseautocomplete-val').val();
        if (val && val !== '0') {
            loadActLawBaseAutoComplete(actLawBaseControl, val);
        }
    });
}

function loadActLawBaseAutoComplete(autoControl, val) {
    $.get(autocompleteUrls.actLawBaseGet + val)
        .done(function (data) {
            $(autoControl).val(data.label);
        }).fail(function (errors) {
            console.log(errors);
        });
}