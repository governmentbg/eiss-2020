$(function () {
    SCS.get(signToolsPath)
        .then(function (json) {
            $('.pdfSigner').find('button.submit-sign-pdf').removeAttr('disabled');
            $('.xmlSigner').find('button.submit-sign-xml').removeAttr('disabled');
        })
        .then(null, function (err) {
            showSignError('ERROR:' + "\r\n" + err.message);
        });

    $(document).on('click', 'button.submit-sign-pdf', function (e) {
        e.preventDefault();
        var signFrm = $(e.currentTarget).parents('form:first');
        let btnSign = $(signFrm).find('button.submit-sign-pdf');
        let signWarning = $(btnSign).data('signwarning');
        if (signWarning && $(btnSign).data('warningshown') != 'true') {
            swalConfirm(signWarning, function () {
                $(btnSign).data('warningshown', 'true');
                $(btnSign).trigger('click');
               
            }, function () {
               
            })
            return;
        }



        $(btnSign).attr('disabled', true);
        var val = $(signFrm).find('#FileHash').val();

        if (!val.length) {
            return showSignError('Не сте въвели хеш за подписване');
        }

        var sid = sessionStorage.getItem('eiss_sign_sid');

        if (!sid) {
            sid = '';
        }

        SCS.signDigestSID(val, sid)
            .then(function (json) {

                $(signFrm).find('#Signature').val(json.signature);
                $(signFrm).find('#ErrorCode').val(json.errorCode);
                sessionStorage.setItem('eiss_sign_sid', json.sid);

                $(signFrm).submit();
            })
            .then(null, function (err) {
                if (checkError) {
                    checkError(err.response.errorCode);
                } else {
                    showSignError('ERROR:' + "\r\n" + err.message);
                }
                
                $(btnSign).removeAttr('disabled');
            });

        return false;
    });

    $(document).on('click', 'button.sign-close', () => $('#signMessageContainer').hide());
});

function showSignError(message) {
    $('#signErrorMessage').text(message);
    $('#signMessageContainer').show();
    $('#signMessageContainer').delay(10000).slideUp(1000);
}

