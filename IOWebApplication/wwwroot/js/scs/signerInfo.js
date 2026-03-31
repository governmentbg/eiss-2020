$(function () {
    //var signToolsPath = "https://sign.uslugi.io/java/stampitls.jnlp";
    SCS.get(signToolsPath)
        .then(function (json) {
            $('#signButton').removeAttr('disabled');
        })
        .then(null, function (err) {
            showSignError('ERROR:' + "\r\n" + err.message);
        });
    if ($('#signButton').data('event-is-set') !== true) {
        $('#signButton').data('event-is-set', true);
        $(document).on('click', '#signButton', async function (e) {
            e.preventDefault();
            e.stopPropagation();
            var signFrm = $('#signForm');
            var signBtn = $('#signButton');
            var visual = $('#cbVisual').is(':checked');
            signBtn.attr('disabled', true);

            try {
                var signInfo = await GetSignInfo(visual);

                if (!signInfo.hash) {
                    throw new Error('Не сте въвели хеш за подписване');
                }

                var data = await SCS.signDigestSID(signInfo.hash, signInfo.sid);

                $(signFrm).find('#Signature').val(data.signature);
                $(signFrm).find('#ErrorCode').val(data.errorCode);
                $(signFrm).find('#TempPdfId').val(signInfo.tempPdfId);
                $(signFrm).submit();

            } catch (error) {
                console.error(error);
                showSignError("Възникна грешка при подписване");
                signBtn.removeAttr('disabled');

                return error;
            }

            return false;
        });
    }
});

function showSignError(message) {
    toastr.error(message);
}

//async function GetSignInfoOld(visual) {
//    var result = {};
//    visual = !!visual;
//    try {
//        var signer = await SCS.selectSigner();
//        var data = {};
//        data.signerName = signer.signerName;
//        data.signerCert = signer.signerCert;
//        result.sid = signer.sid;
//        data.reason = $('#signForm').find('#Reason').val();
//        data.fileId = $('#signForm').find('#FileId').val();
//        data.visual = visual;

//        var res = await fetch('/files/preparePdfForSignature', {
//            method: 'POST',
//            headers: {
//                'Content-Type': 'application/json',
//            },
//            body: JSON.stringify(data),
//        });

//        var signInfo = await res.json();
//        result.hash = signInfo.hash;
//        result.tempPdfId = signInfo.tempPdfId;
//    } catch (e) {
//        console.error(e);
//        throw new Error(e);
//    }

//    return result;
//}

async function GetSignInfo() {
    var result = {};
    try {
        var signer = await SCS.selectSigner();

        //var data = {};
        result.signerName = signer.signerName;
        result.signerCert = signer.signerCert;
        return result;
        //result.sid = signer.sid;
        //data.reason = $('#signForm').find('#Reason').val();
        //data.fileId = $('#signForm').find('#FileId').val();
        //data.visual = visual;

        //var res = await fetch('/files/preparePdfForSignature', {
        //    method: 'POST',
        //    headers: {
        //        'Content-Type': 'application/json',
        //    },
        //    body: JSON.stringify(data),
        //});

        //var signInfo = await res.json();
        //result.hash = signInfo.hash;
        //result.tempPdfId = signInfo.tempPdfId;
    } catch (e) {
        console.error(e);
        throw new Error(e);
    }

    return result;
}


