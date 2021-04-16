//Връща стринг, като премахва всички тагове <script> от подадения текст
//Константин Борисов,08.2013
function stripScripts(s) {
    var div = document.createElement('div');
    div.innerHTML = s;
    var scripts = div.getElementsByTagName('script');
    var i = scripts.length;
    while (i--) {
        scripts[i].parentNode.removeChild(scripts[i]);
    }
    return div.innerHTML;
}

//Връща html.escape стринг, в който са заменени специалните символи, чупещи request-а
//Константин Борисов,08.2013
function htmlEscape(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;')
        .replace(/#/g, '&dies&')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;');
}

//Връща стринг, с преобразувани специални символи (обратното на htmlEscape)
//Константин Борисов,08.2013
function htmlUnescape(value) {
    return String(value)
        .replace(/&quot;/g, '"')
        .replace(/&dies&/g, '#')
        .replace(/&#39;/g, "'")
        .replace(/&lt;/g, '<')
        .replace(/&gt;/g, '>')
        .replace(/&amp;/g, '&');
}

//Връща HTML като стринг, който отговаря на съдържанието на подадения контейнер, с преобразувани стойности и премахнати <script> тагове
//Константин Борисов,08.2013
function GetEncodedContent(containerSelector, page_title) {
    return htmlEscape(ReplaceControlsInContent(containerSelector, page_title));
}
function ReplaceControlsInContent(containerSelector, page_title) {

    var error = '';
    var clone = '';
    try {
        clone = $(containerSelector).clone();
    } catch (e) {

        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        var textareas = $(containerSelector).find("textarea");
        $(textareas).each(function (i) {
            var tarea = this;
            $(clone).find("textarea").eq(i).val($(tarea).val());
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }


    try {
        var selects = $(containerSelector).find("select");
        $(selects).each(function (i) {
            var select = this;
            $(clone).find("select").eq(i).val($(select).val());
        });

        clone.find('select').replaceWith(function () {
            return "&nbsp;<b>" + $(this).find('option:selected').text() + "</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        clone.find('div.git_edit_control').replaceWith(function () {
            var _val = $(this).find('input.git_edit_text').val();
            return "&nbsp;<b>" + _val + "</b>&nbsp;";
        }
        );
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }

    try {
        clone.find(':input[type="text"]').replaceWith(function () {
            return "&nbsp;<b>" + $(this).val() + "</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        clone.find(':input[type="checkbox"]').replaceWith(function () {
            if ($(this).is(":checked"))
                //return "&nbsp;<b>[X]</b>&nbsp;";
                return '<i class="fa  fa-check-square-o"></i>';
            else
                return '<i class="fa  fa-square-o"></i>';
            //return "&nbsp;<b>[&nbsp;]</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        clone.find(':input[type="radio"]').replaceWith(function () {
            if ($(this).is(":checked"))
                return "&nbsp;<b>(X)</b>&nbsp;";
            else
                return "&nbsp;<b>(&nbsp;)</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        clone.find(':input[type="number"]').replaceWith(function () {
            return "&nbsp;<b>" + $(this).val() + "</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }

    try {
        clone.find('textarea').replaceWith(function () {
            var _val = $(this).val();
            //debugger;
            return "&nbsp;<b>" + _val + "</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }

    try {
        clone.find('button.ui-datepicker-trigger').remove();
        clone.find('div#divValidationSummary').remove();

        clone.find(':input[type="hidden"]').remove();
        clone.find(':input[type="submit"]').remove();
        clone.find(':input[type="reset"]').remove();
        clone.find(':input[type="button"]').remove();
        clone.find('a[data-toggle!="tab"]').removeAttr('src').attr('href', '#').removeAttr('click');
        clone.find('a[data-toggle="tab"]').each(function (i, e) {
            let tab = clone.find('#' + $(e).attr('href').replace('#', ''));
            $(tab).attr('id', $(tab).attr('id') + '_log');
            $(e).attr('href', $(e).attr('href') + '_log');
        });
        clone.find('a.btn,a.icon,a.button').remove();
        clone.find('span.text-custom-danger').remove();
        clone.find('a.clearInput,a.auto-cmpt-search').remove();
        clone.find('div.input-group-addon').remove();
        clone.find('label.time-toggle').remove();
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        clone.find('label').removeAttr('for');
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }



    try {
        clone.find(':input[type="file"]').replaceWith(function () {
            var _val = $(this).attr('value');
            return "&nbsp;<b>" + _val + "</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }
    try {
        clone.find('input.ui-autocomplete-input').replaceWith(function () {
            var _val = $(this).val();
            return "&nbsp;<b>" + _val + "</b>&nbsp;";
        });
    } catch (e) {
        error = error + e.name + ': ' + e.message + '  ; ';
    }


    clone.find('div.form-group').each(function (i, e) {
        $(e).attr('data-container', i);
    });

    var _result = stripScripts((clone.html()));

    return _result;
}
var jsonPageSize = 10;
function MakeJsonPager(data, pager_container, page_no, callbackName) {
    var pageCount = Math.ceil(data.length / jsonPageSize);
    var pagerData = [];
    for (var i = 1; i <= pageCount; i++) {
        var _selected = false;
        if (i === page_no) {
            _selected = true;
        }
        var _new = { page: i, selected: _selected };
        pagerData.push(_new);
    }
    var pagerTemplate = '{{#each this}}<a href="#" class="btn {{#if selected}}light{{/if}}{{#unless selected}}dark{{/unless}}" onclick="' + callbackName + '({{page}});return false;">{{page}}</a>{{/each}}';
    var pagerHtml = HandlebarsToHtml(pagerTemplate, pagerData);
    if (pageCount === 1)
        pagerHtml = '';
    $(pager_container).html(pagerHtml);

    var startRow = Math.max((page_no - 1) * jsonPageSize, 0);
    //if (page_no == 1)
    var endRow = startRow + jsonPageSize;

    return data.slice(startRow, endRow);
}

function LogOperationCompareVersions(current, prior) {
    if (prior == '' || prior.length == 0) {
        //alert('prior is empty!');
        return current;
    }
    var currentNode = $.parseHTML(current);
    var priorNode = $.parseHTML(prior);
    //debugger;
    //alert($(currentNode).find('div.form-group').length);
    //alert($(priorNode).find('div.form-group').length);

    $(currentNode).find('div.form-group').each(function (i, e) {
        var priorDiv = $(priorNode).find('div.form-group[data-container="' + $(e).data('container') + '"]:first');
        //if ($(priorDiv[0]).data('container') == '7') {
        //    var currentInnerHtml = $(e).html();
        //    var priorInnerHtml = $(priorDiv[0]).html();
        //    debugger;
        //}
        if ($(e).html() != $(priorDiv[0]).html()) {
            $(e).addClass('changed');
        }
    });
    //debugger;
    var wrapper = document.createElement('div');
    $(currentNode).each(function (i, e) {
        wrapper.appendChild(e);
    });

    var result = wrapper.outerHTML;

    return result;
}