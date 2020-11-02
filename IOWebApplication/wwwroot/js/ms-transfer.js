(function ($) {
    'use strict';

    var MultiSelectionTransfer = (function ($) {
        function MultiSelectionTransfer(options, $container) {
            var self = this;
            self.left_arr = new Array();
            self.right_arr = new Array();
            self.$container = $container;
            var id = $container.prop('id');
            var havePercent = false;
            if ((options) && (options.havePercent))
                havePercent = true;
            var aTemplateRight = havePercent ? "#templatePercentRight" : "#templateRight";
            if (options && options.templateRight) {
                aTemplateRight = options.templateRight;
            }
            var aTemplateRightModal = "";
            if (options && options.templateRightModal) {
                aTemplateRightModal = options.templateRightModal;
            }
            self._opts = $.extend({
                ajaxLeftUrl: "",
                ajaxRightUrl: "",
                ajaxAllUrl: "",
                havePercent: false,
                percent: 100,
                selectLeft: "#" + id + "_Left",
                templateRight: aTemplateRight,
                templateRightModal: aTemplateRightModal,
                divRightEdit: '#divRightEdit',
                templateErrMultiTransfer: "#templateErrMultiTransfer",
                divRight: "#" + id + "_Right",
                btnMoveSelectedRight: "#" + id + "_MoveSelectedRight",
                inputPercent: "#" + id + "_Percent",
                errMsgPercentAll: "Въведете натовареност процент от 1% до 100%",
                errMsgPercent: "Има невалидни проценти натовареност"
            }, options);
            self.init();
        }
        MultiSelectionTransfer.prototype = {
            init: function () {
                var self = this;
                self.setMoveBtnsListener();
                self.setPercentCheck();
                self.$container.find(self._opts.inputPercent).val(self._opts.percent);
                if (!self._opts.havePercent)
                    self.$container.find('.ms-transfer-all-percent').hide();
            },
            deleteRightItem: function (id) {
                var self = this;
                self.right_arr = self.right_arr.filter(function (x) {
                    return x.id != id;
                });
                self.showLeft();
                self.showRight();
            },
            setDeleteRightItem: function () {
                var self = this;
                $(self.$container.find('.ms-transfer-right-delete')).each(
                    function () {
                        var idStr = this.id; var idStr = this.id;
                        var id = parseInt(idStr.replace("delete", ""));
                        this.onclick = function () {
                            self.deleteRightItem(id);
                        };
                    }
                );
            },
            editRightItem: function (id) {
                var self = this;
                var right_edit = self.right_arr.find(function (x) {
                    return x.id == id;
                });
                var saveEdit = function () {
                    if (typeof right_edit.old_percent === "undefined")
                        right_edit.old_percent = right_edit.percent;
                    right_edit.percent = $(divRightEdit).find('#percent_modal').val();
                    right_edit.reason = $(divRightEdit).find('#reason_modal').val();
                    right_edit.comment = right_edit.reason + " предишен % " + right_edit.old_percent;
                }
                var saveDelete = function () {
                    right_edit.is_delete = true;
                    right_edit.reason = $(divRightEdit).find('#reason_modal').val();
                    right_edit.comment = "ИЗТРИВАНЕ " + right_edit.reason;
                }
                var editHtml = TemplateToHtml(self._opts.templateRightModal, right_edit);
                var divRightEdit = self._opts.divRightEdit;
                $(divRightEdit).html(editHtml);
                $(divRightEdit).find('#button_save_modal').click(function () {
                    var is_ok_percent = self.percentCheckModal();
                    var is_ok_reason = self.reasonCheckModal();
                    if (is_ok_reason && is_ok_percent) {
                        saveEdit();
                        self.showLeft();
                        self.showRight();
                        $(divRightEdit).modal('hide');
                    }
                });
                $(divRightEdit).find('#button_delete_modal').click(function () {
                    var is_ok_reason = self.reasonCheckModal();
                    if (is_ok_reason) {
                        saveDelete();
                        self.showLeft();
                        self.showRight();
                        $(divRightEdit).modal('hide');
                    }
                })

                $(divRightEdit).modal('show');
            },
            setEditRightItem: function () {
                var self = this;
                $(self.$container.find('.ms-transfer-right-edit')).each(
                    function () {
                        var idStr = this.id; var idStr = this.id;
                        var id = parseInt(idStr.replace("edit", ""));
                        this.onclick = function () {
                            self.editRightItem(id);
                        };
                    }
                );
            },
            setOrderPlus: function (arr) {
                arr.forEach(function (x) {
                    x.orderPlus = ("0000000" + x.order).slice(-7) + x.text.toLowerCase();
                })
            },
            loadLeft: function (data_in) {
                var self = this;
                $.ajax({
                    type: "GET",
                    url: self._opts.ajaxLeftUrl,
                    dataType: "json",
                    cache: false,
                    data: data_in,
                    success: function (response) {
                        self.left_arr = response;
                        self.setOrderPlus(self.left_arr);
                        self.showLeft();
                    },
                    complete: function () { },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log("ajaxLeftUrl:" + self._opts.ajaxLeftUrl)
                        console.log({ jqXHR: jqXHR });
                        console.log({ textStatus: textStatus });
                        console.log({ errorThrown: errorThrown });
                    }
                });
            },
            loadRight: function (data_in) {
                var self = this;
                $.ajax({
                    type: "GET",
                    url: self._opts.ajaxRightUrl,
                    dataType: "json",
                    cache: false,
                    data: data_in,
                    success: function (response) {
                        self.right_arr = response;
                        self.setOrderPlus(self.right_arr);
                        self.showRight();
                        self.showLeft();
                    },
                    complete: function () { },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log("ajaxRightUrl : " + self._opts.ajaxRightUrl);
                        console.log({ jqXHR: jqXHR });
                        console.log({ textStatus: textStatus });
                        console.log({ errorThrown: errorThrown });
                    }
                });
            },
            clearRight: function () {
                var self = this;
                self.right_arr.length = 0;
                self.showRight();
            },
            loadAll: function (data_in) {
                var self = this;
                $.ajax({
                    type: "GET",
                    url: self._opts.ajaxAllUrl,
                    dataType: "json",
                    cache: false,
                    data: data_in,
                    success: function (response) {
                        self.left_arr = response.dataLeft;
                        self.setOrderPlus(self.left_arr);
                        self.right_arr = response.dataRight;
                        self.setOrderPlus(self.right_arr);
                        self.showRight();
                        self.showLeft();
                    },
                    complete: function () { },
                    error: function (jqXHR, textStatus, errorThrown) {
                        console.log("ajaxAllUrl: " + self._opts.ajaxAllUrl);
                        console.log({ jqXHR: jqXHR });
                        console.log({ textStatus: textStatus });
                        console.log({ errorThrown: errorThrown });
                    }
                });
            },
            showLeft: function () {
                var self = this;
                var right_arr_id = self.right_arr.map(function (x) {
                    return x.id;
                });
                var left_tmp_arr = self.left_arr.filter(function (x) {
                    return right_arr_id.indexOf(x.id) < 0;
                });
                if (self.left_filter && self.left_filter !== "") {
                    left_tmp_arr = left_tmp_arr.filter(function (x) {
                        return x.text.toLowerCase().indexOf(self.left_filter.toLowerCase()) !== -1;
                    });
                }
                left_tmp_arr.sort(sort_by('orderPlus', false, function (a) { return a }));
                var tempStr = left_tmp_arr.map(
                    function myFunction(value) {
                        return '<option title="' + value.text + '" value="' + value.id + '">' + value.text + "</option>";
                    }
                )
                    .join();
                self.$container.find(self._opts.selectLeft).html(tempStr);
            },
            showRight: function () {
                var self = this;
                self.right_arr.sort(sort_by('orderPlus', false, function (a) { return a }));
                var newHtml = TemplateToHtml(self._opts.templateRight, { data: self.right_arr });
                self.$container.find(self._opts.divRight).html(newHtml);
                self.setPercentItemCheck();
                self.setDeleteRightItem();
                self.setEditRightItem();
            },
            filterLeft: function (newFilter) {
                var self = this;
                self.left_filter = newFilter;
                self.showLeft();
            },
            moveSelectedLeftToRight: function () {
                var self = this;
                if (!self.isIntegerPercent(self.$container.find(self._opts.inputPercent).val())) {
                    console.log(self._opts.templateErrMultiTransfer);
                    console.log(self._opts.errMsgPercentAll);

                    var newHtml = TemplateToHtml(self._opts.templateErrMultiTransfer, self._opts.errMsgPercentAll);
                    $("#messageContainer").html(newHtml);
                    $("#messageContainer").show();
                    $('#messageContainer').delay(10000).slideUp(1000);
                    return;
                };
                var leftList = self.$container.find(self._opts.selectLeft);
                var selector = leftList.find("option:selected");
                var move_arr = new Array();
                selector.each(function () {
                    move_arr.push(parseInt($(this).val()));
                });

                var right_arr_id = self.right_arr.map(function (x) {
                    return x.id;
                });
                move_arr = move_arr.filter(function (x) {
                    return right_arr_id.indexOf(x.id) < 0;
                });
                self.left_arr.forEach(function (x) {
                    if (move_arr.indexOf(x.id) >= 0) {
                        x.percent = self._opts.percent;
                        self.right_arr.push(x);
                    };
                });
                self.showLeft();
                self.showRight();
            },
            setMoveBtnsListener: function () {
                var self = this;
                $(self._opts.btnMoveSelectedRight).click(function () { self.moveSelectedLeftToRight() });
            },

            getSelectedId: function () {
                var self = this;
                var right_arr_id = self.right_arr.map(function (x) {
                    return x.id;
                });
                return JSON.stringify(right_arr_id);
            },
            getSelected: function () {
                var self = this;
                var right_arr_id = self.right_arr.map(function (x) {
                    return { id: x.id, percent: x.percent, reason: x.reason, isDelete: x.is_delete };
                });
                return JSON.stringify(right_arr_id);
            },
            isIntegerPercent: function (value) {
                var x = parseInt(value);
                return (x.toString() == value.trim()) &&
                    (typeof x === 'number') &&
                    (x % 1 === 0) && (x > 0) && (x <= 100);
            },
            percentItemCheck: function (e) {
                var self = this;
                var err = "*";
                if (self.isIntegerPercent(e.target.value)) {
                    err = "";
                    var idStr = e.target.id.replace("percent", "");
                    var percent = parseInt(e.target.value)
                    self.right_arr.forEach(function (item) {
                        if (item.id == idStr)
                            item.percent = percent;
                    });
                }
                self.$container.find("#" + e.target.id + "_val").html(err);
            },
            percentCheck: function (e) {
                var self = this;
                var err = self._opts.errMsgPercentAll;
                if (self.isIntegerPercent(e.target.value)) {
                    err = "";
                    self._opts.percent = parseInt(e.target.value);
                }
                self.$container.find("#" + e.target.id + "_val").html(err);
            },
            setPercentCheck: function () {
                var self = this;
                $(self._opts.inputPercent).keyup(
                    function (event) { self.percentCheck(event); }
                );
            },
            setPercentItemCheck: function () {
                var self = this;
                self.$container.find(".ms-transfer-right-item-percent-input").keyup(
                    function (event) { self.percentItemCheck(event); }
                );
            },
            haveInValidpercent: function () {
                var self = this;
                var result = false;
                self.right_arr.forEach(
                    function (x) {
                        if (self.$container.find("#percent" + x.id.toString() + "_val").html() == "*")
                            result = true;
                    });
                if (result === true)
                    self.showErrTemplate(self._opts.errMsgPercent);
                return result;
            },
            showErrTemplate: function (errMsg) {
                var self = this;
                var newHtml = TemplateToHtml(self._opts.templateErrMultiTransfer, errMsg);
                $("#messageContainer").html(newHtml);
                $("#messageContainer").show();
                $('#messageContainer').delay(10000).slideUp(1000);
            },
            percentCheckModal: function () {
                var self = this;
                var err = self._opts.errMsgPercentAll;
                if (self.isIntegerPercent($('#percent_modal').val())) {
                    err = "";
                }
                $("#percent_modal_span").html(err);
                return err == "";
            },
            reasonCheckModal: function () {
                var self = this;
                var err = "Въведете причина";
                var reason = $('#reason_modal').val();
                console.log(reason);
                if (reason != null && reason != "") {
                    err = "";
                }
                $("#reason_modal_span").html(err);
                return err == "";
            },
        }

        var sort_by = function (field, reverse, primer) {
            var key = primer ?
                function (x) { return primer(x[field]) } :
                function (x) { return x[field] };

            reverse = !reverse ? 1 : -1;

            return function (a, b) {
                return a = key(a), b = key(b), reverse * ((a > b) - (b > a));
            }
        }

        return MultiSelectionTransfer;
    })($);
    $.fn.multiselectiontransfer = function (options) {
        var $this = $(this),
            data = $this.data('multiselectiontransfer');
        if (!data) {
            $this.data('multiselectiontransfer', (data = new MultiSelectionTransfer(options, $this)));
        }
        return data;
    };
})(jQuery);