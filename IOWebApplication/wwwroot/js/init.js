(function () {
    // DataTables global settings

    $.fn.dataTable.ext.buttons.io_excel = {
        extend: 'excel',
        text: '<i class="fa fa-file-excel-o"></i>',
        titleAttr: 'Excel',
        className: 'btn-default',
        exportOptions: {
            "columns": "thead th:not(.noExport)",
            "columns": ":visible"
        }
    };

    $.fn.dataTable.ext.buttons.io_pdf = {
        extend: 'collection',
        text: '<i class="fa fa-file-pdf-o"></i>',
        titleAttr: 'Pdf',
        className: 'btn-default',
        autoClose: true,
        buttons: [
            {
                extend: 'pdf',
                text: 'Портретно',
                exportOptions: {
                    "columns": "thead th:not(.noExport)"
                },
                orientation: 'portrait'
            },
            {
                extend: 'pdf',
                text: 'Пейзажно',
                exportOptions: {
                    "columns": "thead th:not(.noExport)"
                },
                orientation: 'landscape'
            }
        ]
    };

    $.fn.dataTable.ext.buttons.io_print = {
        extend: 'print',
        text: '<i class="fa fa-print"></i>',
        titleAttr: 'Печат',
        className: 'btn-default',
        exportOptions: {
            "columns": "thead th:not(.noExport)"
        }
    };

    $.fn.dataTable.ext.buttons.io_colvis = {
        extend: 'colvis',
        text: '<i class="fa  fa-eye-slash"></i>',
        titleAttr: 'Видими Колони',
        className: 'btn-default'
    };
    $.fn.dataTable.ext.buttons.io_pageLength = {
        extend: 'pageLength',
        className: 'btn-default'
    };

    $.extend(true, $.fn.dataTable.defaults, {
        "initComplete": function () {
            // Search form events
            var initSearchForm = $('.search-form');
            var initTable = $('.dataTable');

            if (initSearchForm.length > 0 && initTable.length > 0) {
                initSearchForm.on('submit', function () {
                    var t = initTable.DataTable();
                    t.state.clear();
                });
            }

            var $searchInput = $('div.dataTables_filter input');
            $searchInput.val('');
            $searchInput.unbind();
            $searchInput.bind('keyup', function (e) {
                if (this.value.length > 2 || this.value === '') {
                    initTable.DataTable().search(this.value).draw();
                }
            });
        },
        dom: '<"row"<"col-sm-6 dataTables_buttons"B><"col-sm-6"f>>rtip',
        buttons: {
            dom: {
                button: {
                    tag: 'button',
                    className: 'btn btn-flat'
                },
                container: {
                    className: ''
                }
            },
            buttons: ['io_pageLength', 'io_colvis', 'io_excel', 'io_pdf', 'io_print']
        },
        "lengthMenu": [
            [10, 25, 50, 100, -1],
            ['10 реда', '25 реда', '50 реда', '100 реда', 'Покажи всички']
        ],
        "bAutoWidth": false,
        "language": {
            "url": "/lib/adminlte/plugins/dataTables/dataTables.bgBG.json"
        },
        fnStateSaveParams: function (settings, data) {
            data.search.search = "";
        },
        filter: true,
        "bLengthChange": false,
        "serverSide": true,
        "processing": true,
        "paging": true,
        "pageLength": 10,
        "stateSave": true,
        "stateDuration": -1
    });

    initDatePicker();

    initPersonControl();
    validateTabs();
    initTinyMCE();
    $(document).on('click', 'a.modal-loader', function () {
        let _url = $(this).data('modal-url');
        let _title = $(this).data('modal-title');
        let bigModal = $(this).hasClass('modal-big');
        requestContent(_url, null, function (html) {
            ShowModalDialog(_title, html, bigModal);
        });
        return false;
    });

    $(document).on("keydown", "form", function (event) {
        if (document.activeElement) {
            if ($(document.activeElement).parents('form:first').hasClass('quick-submit')) { return true; }
        }
        return event.key != "Enter" || document.activeElement.tagName == 'TEXTAREA';
    });

    $(document).on("keydown", "div.filter-form", function (event) {
        if (event.key == "Enter") {
            if (!$(document.activeElement).hasClass('ui-autocomplete-input')) {
                $(document.activeElement).parents('div.filter-form:first').find('.filter-button').trigger('click');
            }
        }
    });

    disableSchrollOnTabsClick();

    $(document).on('click', '.single-click-submit', function (e) {
        singleClickSubmitDisable(this);
        e.preventDefault();

        if (!$(this).parents('form:first').valid()) {
            singleClickSubmitEnable();
        }
        return false;
    });



    loadDatatablesOnShow();
    mqInfoLoad();
})();

function loadDatatablesOnShow() {
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (tabLink) {

        //debugger;
        var target = $(tabLink.target).attr("href");
        var tabPane = $(this).parents('div.nav-tabs-custom:first').find(target);

        if ($(tabPane).hasClass('dt-lazy-loading')) {
            $(tabPane).removeClass('dt-lazy-loading').find('table.dataTable').each(function (i, e) {
                $(e).DataTable().ajax.reload();
            });
        }
    });
}

function deferLoadingOnTab(sender) {
    var tabPans = $(sender).parents('div.tab-pane:first');
    if (tabPans.hasClass('dt-lazy-loading') && !tabPans.hasClass('active')) {
        return 0;
    } else {
        return null;
    }
}

function disableSchrollOnTabsClick() {
    $('ul.nav-tabs li a').click(function (e) {
        //return false;
        //e.stopPropagation();
    });
}

function initTinyMCE() {
    tinymce.init({
        selector: 'textarea.tinymce',
        language: 'bg_BG',
        height: 600,
        menubar: 'file edit view insert format tools table tc help',
        plugins: [
            'advlist autolink lists link image charmap print preview anchor',
            'searchreplace visualblocks code fullscreen',
            'insertdatetime media table powerpaste code help wordcount'
        ],
        browser_spellcheck: true,
        contextmenu: false,
        //Така запазва всички стилове при копиране от Word
        paste_retain_style_properties: "all",
        //valid_styles: { '*': 'color,font-size,font-weight,font-style,text-indent,text-decoration,text-align,border,margin,padding,line-height,page-break-before,margin-left,margin-right,margin-top,margin-bottom,text-transform' },
        // - без margin-left;margin-right връщаме
        valid_styles: { '*': 'color,font-size,font-weight,font-style,text-indent,text-decoration,text-align,border,padding,line-height,page-break-before,margin-top,margin-bottom,text-transform,margin-left,margin-right' },
        //paste_word_valid_elements: "b,strong,i,em,h1,h2,u,p,ol,ul,li,a[href],span,color,font-size,font-color,font-family,mark",
        //toolbar: 'undo redo | formatselect | bold italic backcolor | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | removeformat | help',
        toolbar: 'undo redo | formatselect | bold italic backcolor | fontselect fontsizeselect formatselect | alignleft aligncenter alignright alignjustify | bullist numlist | removeformat | help',
        visual_table_class: "table",
        content_css: "/css/tinymce-generic-styles.css",
        //indentation: '30pt',
        table_default_attributes: { border: 0 },
        setup: function (editor) {
            editor.on('blur', function () {
                editor.save();
            });
            editor.on('keydown', function (evt) {
                console.debug('Key up event: ' + evt.keyCode);
                var keyCode = evt.keyCode || evt.which;
                if (keyCode == 9) { // tab pressed
                    editor.execCommand('mceInsertContent', true, '&nbsp;&nbsp;&nbsp;&nbsp;');
                    //editor.execCommand('mceInsertRawHTML', false, '<span>&nbsp;&nbsp;&nbsp;</span>');
                    evt.preventDefault();
                    evt.stopPropagation();
                    return false;
                }
            });
        }
    });
}

function validateTabs() {
    $('ul.nav-tabs a').each(function (i, e) {
        var tabContent = $(e).attr('href');
        //debugger;
        if ($(tabContent).find('.input-validation-error').length > 0) {
            if (!$(this).hasClass('tab-validation-error')) {
                $(this).addClass('tab-validation-error');
            }
        } else {
            $(this).removeClass('tab-validation-error');
        }
        if ($(tabContent).find('.io-element-warning').length > 0) {
            if (!$(this).hasClass('tab-validation-warning')) {
                $(this).addClass('tab-validation-warning');
            }
        } else {
            $(this).removeClass('tab-validation-warning');
        }
    });
}
function initPersonControl() {
    $('.person--control select[id$="UicTypeId"]').change(function () {
        let uicType = $(this).val();
        $(this).parents('.person--control:first').find('.person-uic').data("uictype", uicType);
        let isEntity = (uicType === '3' || uicType === '5');
        if (isEntity) {
            $(this).parents('.person--control:first').find('.entity--names').show();
            $(this).parents('.person--control:first').find('.person--names').hide();
            $(this).parents('.person--control:first').find('.row-deceased').hide();
        } else {
            $(this).parents('.person--control:first').find('.entity--names').hide();
            $(this).parents('.person--control:first').find('.person--names').show();
            $(this).parents('.person--control:first').find('.row-deceased').show();
        }
    }).trigger('change');

    $('.person--control input[id$="Uic"]').change(function () {
        let uic = $(this).val();
        let personControl = $(this).parents('.person--control:first');
        let uicType = personControl.find('select[id$="UicTypeId"]').val();
        if (uic && uic.length > 3) {
            requestGET_Json(rootDir + "ajax/FindPersonByUic", { uic: uic, uicType: uicType }, function (data) {
                if (data) {
                    personControl.find('input[id$="FirstName"]').val(data.firstName);
                    personControl.find('input[id$="MiddleName"]').val(data.middleName);
                    personControl.find('input[id$="FamilyName"]').val(data.familyName);
                    personControl.find('input[id$="Family2Name"]').val(data.family2Name);
                    personControl.find('input[id$="FullName"]').val(data.fullName);
                    personControl.find('input[id$="LatinName"]').val(data.latinName);
                    personControl.find('input[id$="DepartmentName"]').val(data.departmentName);
                }                
            });
        }
    });
    $('.person--control input[id$="IsDeceased"]').change(function () {
        let isDeceased = $(this).is(':checked');
        let personControl = $(this).parents('.person--control:first');
        if (isDeceased) {
            personControl.find('div.date-deceased').show();
        } else {
            personControl.find('div.date-deceased').hide().find('input.date-picker').val('');
        }
    }).trigger('change');


    initPersonSearch();
}

var personSearchData = [];

function initPersonSearch() {
    $('.person--control').each(function (i, e) {
        let personControl = $(e);
        $(e).find('.uic--search').click(function () {
            let uicType = personControl.find('select[id$="UicTypeId"]').val();
            let uic = personControl.find('input[id$="Uic"]').val();
            let regix_caseid = $('#RegixRequestReason_RegixReasonCaseId').val();
            let regix_documentid = $('#RegixRequestReason_RegixReasonDocumentId').val();
            let regix_description = $('#RegixRequestReason_RegixReasonDescription').val();
            let regix_guid = $('#RegixRequestReason_RegixReasonGuid').val();
            let regix_type_id = $('#RegixRequestReason_RegixRequestTypeId').val();

            requestGET_Json(rootDir + "RegixReport/PersonSearch", {
                uic: uic, uicType: uicType,
                regixReasonDocumentId: regix_documentid, regixReasonCaseId: regix_caseid,
                regixReasonDescription: regix_description, regixReasonGuid: regix_guid,
                regixRequestTypeId: regix_type_id
            }, function (data) {
                personSearchData = data;
                let _template = '{{#if this}}{{#each this}}<p><h4>{{registerName}}</h4>{{#if fullName}} Наименование: <b>{{fullName}}</b>{{else}} Имена: <b>{{firstName}} {{middleName}} {{familyName}}</b>{{/if}} <a href="#" onclick="selectPersonSearchData(this,\'{{id}}\',\'' + $(personControl).data('controlid') + '\');return false;"  class="btn btn-xs btn-primary select-person-data">Избери</a></p><hr/>{{/each}}{{else}}<p>Няма намерени данни.</p>{{/if}}';
                let _html = HandlebarsToHtml(_template, personSearchData);
                ShowModalDialog('Проверка за идентификатор ' + uic, _html);
            });
            return false;
        });
    });
}

function selectPersonSearchData(sender, personId, controlId) {
    let personData = personSearchData.filter(function (person) { return person.id === personId; })[0];
    let personControl = $('.person--control[data-controlid="' + controlId + '"]');
    switch (personData.uicTypeId) {
        case 1: {
            $(personControl).find('input[id$="FirstName"]').val(personData.firstName);
            $(personControl).find('input[id$="MiddleName"]').val(personData.middleName);
            $(personControl).find('input[id$="FamilyName"]').val(personData.familyName);
            $(personControl).find('input[id$="FamilyName2"]').val('');
            if (personData.isDead) {
                $(personControl).find('input[id$="IsDeceased"]').prop('checked', 'checked').trigger('change');
                $(personControl).find('input[id$="DateDeceased"]').val(personData.deathDate);
            }

            if (typeof appendAddressToDocumentPersonFromNBD === 'function') {
                appendAddressToDocumentPersonFromNBD(personData.uic, controlId);
            }
        }
            break;
        default: {
            $(personControl).find('input[id$="FullName"]').val(personData.fullName);
        }
            break;
    }
    HideModal();
}


function initDatePicker() {
    $('.date-picker').datepicker({
        todayHighlight: true,
        autoclose: true,
        //showOn: "button",
        //showButtonPanel: true,
        format: 'dd.mm.yyyy',
        language: 'bg-BG',
        orientation: 'bottom'
    });

    $(".date-picker").keyup(function (e) {
        if (e.keyCode == 110) {
            var temp = $(this).val();
            var tam = $(this).val().length;
            $(this).val(temp.substring(0, tam - 1));
        }
        else if (e.keyCode != 8) {
            if ($(this).val().length == 2) {
                $(this).val($(this).val() + ".");
            } else if ($(this).val().length == 5) {
                $(this).val($(this).val() + ".");
            }
        }
    });

    $('.dateyear-picker').datepicker({
        todayHighlight: true,
        autoclose: true,
        format: 'yyyy',
        language: 'bg-BG',
        viewMode: 'years',
        minViewMode: 'years',
        maxDate: '2030',
        minDate: '1900'
    });

    $('.datemonthyear-picker').datepicker({
        todayHighlight: true,
        autoclose: true,
        format: 'mm.yyyy',
        language: 'bg-BG',
        viewMode: 'months',
        minViewMode: 'months'
    });

    $('.datetime-picker').datetimepicker({
        format: 'DD.MM.YYYY HH:mm',
        locale: 'bg-BG'
    });

    $('.time-picker').datetimepicker({
        format: 'HH:mm',
        locale: 'bg-BG'
    });

    $('.date-range-picker').daterangepicker({
        locale: {
            direction: 'ltr',
            format: 'DD.MM.YYYY',
            separator: ' - ',
            applyLabel: 'Избери',
            cancelLabel: 'Отказ',
            weekLabel: 'С',
            customRangeLabel: 'Период',
            daysOfWeek: ["П", "В", "С", "Ч", "П", "С", "Н"],
            monthNames: ["Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември"],
            firstDay: 1
        }
    });

    $('.datetime-divided').each(function (i, e) {
        $(e).parents('form:first').submit(function () {
            let dateVal = getDevidedDateTimeValue(undefined, e);
            $(e).find('.model-val').val(dateVal);


            //let dateVal = $(e).find('.date-picker').val();
            //let timePicker = $(e).find('.time-picker')[0];
            //if (!dateVal) {
            //    $(e).find('.model-val').val(null);
            //    return;
            //}
            //if (!$(timePicker).parent().hasClass('visibility_hidden')) {
            //    let timeVal = $(timePicker).val();
            //    let date = moment(dateVal + ' ' + timeVal, "DD.MM.YYYY HH:mm");
            //    $(e).find('.model-val').val(date.format('YYYY-MM-DD HH:mm'));
            //} else {
            //    $(e).find('.model-val').val(moment(dateVal, "DD.MM.YYYY").format('YYYY-MM-DD'));
            //}


        });
        $(e).find('.time-toggle').click(function (el) {
            $(el.currentTarget).parent().toggleClass('visibility_hidden');
        });
    });
}


function getDevidedDateTimeValue(hiddenDateControl, e) {
    if (!e || (e === undefined)) {
        e = $(hiddenDateControl).parents('.datetime-divided:first');
    }
    let dateVal = $(e).find('.date-picker').val();
    let timePicker = $(e).find('.time-picker')[0];
    if (!dateVal) {
        return null;
    }
    if (!$(timePicker).parent().hasClass('visibility_hidden')) {
        let timeVal = $(timePicker).val();
        let date = moment(dateVal + ' ' + timeVal, "DD.MM.YYYY HH:mm");
        return date.format('YYYY-MM-DD HH:mm');
    } else {
        return moment(dateVal, "DD.MM.YYYY").format('YYYY-MM-DD');
    }
}

var loader = function () {
    const show = () => { $('#ajaxLoader').show() };
    const hide = () => { $('#ajaxLoader').hide() };

    return {
        show,
        hide
    }
}();