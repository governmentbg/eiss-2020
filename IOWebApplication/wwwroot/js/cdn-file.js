function reloadCdnFileList(containerId) {
    var url = rootDir + "files/GetFileList";
    var container = document.getElementById(containerId);
    var sType = $(container).data('type');
    var sId = $(container).data('id');
    $.post(url, { sourceType: sType, sourceId: sId }, function (data) {
    });
}