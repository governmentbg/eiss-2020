$(function () {
    $(document).on('click', '.xmlSigner button.submit-sign-xml', function (e) {
        e.preventDefault();
        var signFrm = $(e.currentTarget).parents('form:first');
        $(signFrm).find('button.submit-sign-xml').attr('disabled', true);
        var val = $(signFrm).find('#FileHash').val();

        if (!val.length) {
            return showSignError('Няма съдържание, подлежащо на подписване');
        }

        //var sid = sessionStorage.getItem('eiss_sign_sid');

        //if (!sid) {
        //    sid = '';
        //}

        SCS.signXML(val)
            .then(function (json) {
                //document.getElementById('result').value = JSON.stringify(json);
                //document.getElementById('Signature').value = ;
                $(signFrm).find('#Signature').val(SCS.Base64Decode(json.signature))

                $(signFrm).find('button.submit-sign-xml').removeAttr('disabled');

                $(signFrm).submit();
            })
            .then(null, function (err) {
                document.getElementById('ErrorCode').value = 'ERROR:' + "\r\n" + err.errorCode;
            });



        //SCS.signDigestSID(val, sid)
        //    .then(function (json) {

        //        $(signFrm).find('#Signature').val(json.signature);
        //        $(signFrm).find('#ErrorCode').val(json.errorCode);
        //        sessionStorage.setItem('eiss_sign_sid', json.sid);

        //        $(signFrm).submit();
        //    })
        //    .then(null, function (err) {
        //        if (checkError) {
        //            checkError(err.response.errorCode);
        //        } else {
        //            showSignError('ERROR:' + "\r\n" + err.message);
        //        }
                
        //        $(signFrm).find('button.submit').removeAttr('disabled');
        //    });

        return false;
    });

    $(document).on('click', 'button.sign-close', () => $('#signMessageContainer').hide());
});

function showSignError(message) {
    $('#signErrorMessage').text(message);
    $('#signMessageContainer').show();
    $('#signMessageContainer').delay(10000).slideUp(1000);
}

