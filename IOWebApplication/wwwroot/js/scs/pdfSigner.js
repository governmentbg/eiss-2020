$(function () {
    var frm = $('.pdfSigner');

    SCS.get(signToolsPath)
        .then(function (json) {
            frm.find('button.submit').removeAttr('disabled');
        })
        .then(null, function (err) {
            showSignError('ERROR:' + "\r\n" + err.message);
        });

    $(document).on('click', 'button.submit', function (e) {
        e.preventDefault();
        var signFrm = $(e.currentTarget).parents('form:first');
        $(signFrm).find('button.submit').attr('disabled', true);
        var val = $(signFrm).find('#PdfHash').val();

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
                
                $(signFrm).find('button.submit').removeAttr('disabled');
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

