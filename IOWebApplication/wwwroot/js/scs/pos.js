var PosUrls = {
    resultSave: rootDir + 'Money/PosPaymentResult?json='
};

$(function () {
    var frm = $('.payment-form');

    SCS.get(signToolsPath)
        .then(function (json) {
            frm.find('button.submit').removeAttr('disabled');
        })
        .then(null, function (err) {
            messageHelper.ShowErrorMessage('Неуспешна инициализация, моля свържете се с администратор');
        });

    //$(document).on('click', 'button.submit', function (e) {
    $(document).off('click', '#btnPay').on('click', '#btnPay', function (e) {
        var posFrm = $(e.currentTarget).parents('form:first');

        var paymentMethod = Number($(posFrm).find('#PaymentTypeId').val());
        var paymentId = Number($(posFrm).find('#Id').val());
        var posResultId = Number($(posFrm).find('#PosPaymentResultId').val());
        var popupForm = $(posFrm).find('#ForPopUp').val();
        var messageContainer = '#messageContainer';
        if (popupForm == 'True') {
            messageContainer = '#messageContainerPayment';
        }

        if (paymentMethod === 1 && paymentId === 0 && posResultId === 0) {
            e.preventDefault();
            $(posFrm).find('button.submit').attr('disabled', true);
            var val = $(posFrm).find('#Amount').val();

            if (!val.length) {
                $(posFrm).find('button.submit').removeAttr('disabled');
                return messageHelper.ShowErrorMessage('Не сте въвели сума за плащане', messageContainer);
            }

            var bankAccountId = Number($(posFrm).find('#CourtBankAccountId').val());
            var ammount = (Number(val.replace(',', '.')) * 100).toFixed(0);
            var senderName = $(posFrm).find('#SenderName').val();

            if (ammount <= 0) {
                $(posFrm).find('button.submit').removeAttr('disabled');
                return messageHelper.ShowErrorMessage('Грешна сума', messageContainer);
            }

            if (bankAccountId <= 0) {
                $(posFrm).find('button.submit').removeAttr('disabled');
                return messageHelper.ShowErrorMessage('Изберете сметка', messageContainer);
            }

            var bankAccounts = jQuery.parseJSON($("#bankAccountJson").val());

            var com = bankAccounts.find(x => x.Id === bankAccountId);
            var comPort = com.ComPortPos;
            if (comPort == "") {
                $(posFrm).find('button.submit').removeAttr('disabled');
                return messageHelper.ShowErrorMessage('Проблем с Comp port', messageContainer);
            }
            
            SCS.posSendPaymentPort(comPort, String(ammount))
                .then(function (json) {
                    // TODO: Да се запише резултата
                    $.post(PosUrls.resultSave + JSON.stringify(json) + "&bankAccountId=" + bankAccountId + "&amount=" + val + "&senderName=" + senderName).done(function (data) {
                        if (data.result == true) {
                            $(posFrm).find('#PosPaymentResultId').val(data.id);
                            $(posFrm).submit();
                            $(posFrm).find('button.submit').removeAttr('disabled');
                        } else {
                            messageHelper.ShowErrorMessage(data.message, messageContainer);
                            $(posFrm).find('button.submit').removeAttr('disabled');
                        }
                    }).fail(function (errors) {
                    });
                })
                .then(null, function (err) {
                    // TODO: Да се запише резултата
                    messageHelper.ShowErrorMessage('Възникна грешка при плащане с POS терминал', messageContainer);

                    $(posFrm).find('button.submit').removeAttr('disabled');
                });

            return false;
        }

        $(posFrm).submit();

        return true;
    });
});


