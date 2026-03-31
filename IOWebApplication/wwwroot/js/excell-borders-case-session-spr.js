(function () {
    // DataTables global settings

    $.fn.dataTable.ext.buttons.io_excel = {
        extend: 'excelHtml5',
        text: '<i class="fa fa-file-excel-o"></i>',
        titleAttr: 'Excel',
        className: 'btn-default',

        createEmptyCells : true,
        customize: function (xlsx) {
            var sheet = xlsx.xl.worksheets['sheet1.xml'];
            $('row c', sheet).attr('s', '25');
            $('row:first c', sheet).attr('s', '51');
            $('c[r*=B]', sheet).attr('s', '67');
            $('row:nth-of-type(2) c', sheet).attr('s', '27');
            $('c[r*=K]', sheet).attr('s', '0');
        },
        exportOptions: {
            "columns": "thead th:not(.noExport)",
            "columns": ":visible"
        }
    };
})();