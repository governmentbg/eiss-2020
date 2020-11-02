jQuery(function ($) {
    $.validator.addMethod('date',
        function (value, element) {

            if (this.optional(element)) {
                return true;
            }
            var ok_array = [
                'DD.MM.YYYY',
                'DD.MM.YYYY HH:mm',
                'DD.MM.YYYY HH:mm:ss',
                'MM.YYYY'
            ].filter(function (type_date) {
                return moment(value, type_date, true).isValid();
            });

            return !!ok_array[0];

        });

    $.validator.addMethod('number', function (value, element) {
        return this.optional(element) || /^-?(?:\d+)(?:(\.|,)\d+)?$/.test(value);
    });

    $.validator.addMethod("enforcetrue", function (value, element, param) {
        return element.checked;
    });

    $.validator.unobtrusive.adapters.addBool("enforcetrue");

    $.validator.methods.range = function (value, element, param) {
        return this.optional(element) || (Number(value.replace(',', '.')) >= Number(param[0]) && Number(value.replace(',', '.')) <= Number(param[1]));
    };

    $.validator.addMethod('person-uic',
        function (value, element) {
            let valOk = true;
            switch ($(element).data('uictype')) {
                case '1':
                    valOk = check_egn(value);
                    break;
                case '4':
                    valOk = check_date(value);
                    break;
                case '3':
                    valOk = check_eik(value);
                    break;
            }
            let title = '';
            if (!valOk) {
                
                switch ($(element).data('uictype')) {
                    case '1':
                        title = 'Невалидно ЕГН.';
                        break;
                    case '4':
                        title = 'Невалидна дата.';
                        break;
                    case '3':
                        title = 'Невалиден ЕИК.';
                        break;
                }
                $(element).attr('title', title);
            } else {
                switch ($(element).data('uictype')) {
                    case '1':
                        title = 'ЕГН';
                        break;
                    case '4':
                        title = 'Дата на раждане';
                        break;
                    case '3':
                        title = 'ЕИК';
                        break;
                }
                $(element).attr('title', title);
            }
            return this.optional(element) || valOk;
        }
        //,
        //function (params, element) {
        //    console.log(params);
        //    console.log(element);
        //    let title = '';
        //    switch ($(element).data('uictype')) {
        //        case '1':
        //            title = 'Невалидно ЕГН.';
        //            break;
        //        case '3':
        //            title = 'Невалиден ЕИК.';
        //            break;
        //        default: return '';
        //    }
        //    $(element).attr('title', title);
        //    console.log(element);
        //    return title;
        //}
    );
});