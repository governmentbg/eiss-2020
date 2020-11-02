$(function () {
    var title = 'Ръководство на потребителя';
    var mobile = (typeof window.orientation !== "undefined") || (navigator.userAgent.indexOf('IEMobile') !== -1);
    $('[nowrap]').removeAttr('nowrap');
    $('[docparttype="Table of Contents"]').remove();
    $('body > div').addClass('pages').wrap('<div class="wrapper">');
    if (!mobile) {
        $('body').addClass('menu');
    }
    $('body').append('<div class="menu-search"></div><div class="menu-side"></div><div class="menu-top"><a href="#" class="button-toc">Съдържание</a><input placeholder="Въведете поне два символа за търсене" /></div><div class="menu-title">'+title+'</div>');
    $('.button-toc').click(function () {
        $('body').toggleClass('menu');
    });
    window.document.title = title;

    var h = [
        '#',
        null,
        null,
        null,
        null,
        null
    ];
    var tree = [];
    $('h1, h2, h3, h4, h5, h6').each(function () {
        var text = $(this).text().replace(/[\s\t\n\r ]+/g, ' ');
        var id = md5(text);
        var level = parseInt(this.tagName.substr(1), 10);
        var parent = '#';
        for (var i = level - 1; i >= 0; i--) {
            if (h[i]) {
                parent = h[i];
                break;
            }
        }
        h[level] = 'tree_' + id;
        tree.push({
            id : 'tree_' + id,
            parent : parent,
            text : text
        });
        this.id = 'node_' + id;
    });
    $('.menu-side').jstree({ core : { data : tree } }).on('changed.jstree', function (e, data) {
        window.location.href = '#' + data.selected[0].replace('tree_', 'node_');
        window.scrollTo(0, $('#' + data.selected[0].replace('tree_', 'node_')).offset().top - 80);
        if (mobile) {
            $('body').removeClass('menu');
        }
    }).on('ready.jstree', function (e, data) { data.instance.open_node(data.instance.get_node('#').children); });
    var hash = window.location.hash.replace('#', '');
    if (hash) {
        window.scrollTo(0, $('#' + hash).offset().top - 80);
    }

    function icontains( elem, text ) {
        return (
            elem.textContent ||
            elem.innerText ||
            $( elem ).text() ||
            ""
        ).toLowerCase().indexOf( (text || "").toLowerCase() ) > -1;
    }
    $.expr.pseudos.icontains = $.expr.createPseudo ?
        $.expr.createPseudo(function( text ) {
            return function( elem ) {
                return icontains( elem, text );
            };
        }) :
        function( elem, i, match ) {
            return icontains( elem, match[3] );
        };
    var to = null;
    var res = null;
    var res_o = null;
    var current = false;
    var last_q = null;
    $('.menu-search').on('click', '.prev', function (e) {
        e.preventDefault();
        var t = window.scrollY;
        if (current !== false) {
            current --;
            if (current < 0) {
                current = res_o.length - 1;
            }
            window.scrollTo(0, res_o[current] - 80);
            $('.menu-search strong').text(current + 1);
        }
    });
    $('.menu-search').on('click', '.next', function (e) {
        e.preventDefault();
        var t = window.scrollY;
        if (current !== false) {
            current ++;
            if (current >= res_o.length) {
                current = 0;
            }
            window.scrollTo(0, res_o[current] - 80);
        } else {
            for (var i = 0; i < res_o.length; i++) {
                if (res_o[i] - 100 > t) {
                    current = i;
                    break;
                }
            }
            window.scrollTo(0, res_o[current] - 80);
        }
        $('.menu-search strong').text(current + 1);
    });
    $('.menu-top input').on('keyup', function (e) {
        clearTimeout(to);
        if (e.which === 13 && res_o.length) {
            return $('.menu-search .next').click();
        }
        to = setTimeout(function () {
            var q = $('.menu-top input').val();
            if (q.length >= 2) {
                if (mobile) {
                    $('body').removeClass('menu');
                }
                if (q !== last_q) {
                    last_q = q;
                    $('.search-result').removeClass('search-result');
                    res = $('p:icontains("'+q+'"), h1:contains("'+q+'"), h2:icontains("'+q+'"), h3:icontains("'+q+'"), h4:icontains("'+q+'"), h5:icontains("'+q+'"), h6:icontains("'+q+'")').addClass('search-result');
                    $('.menu-search')
                        .html(( res.length ? '<button class="prev">&laquo;</button><button class="next">&raquo;</button> ' : '') + '<span>' + (res.length ? res.length + ' съвпадения' : 'Няма резултати') + '</span>')
                        .show();
                    if (res.length) {
                        $('.menu-search')
                            .html('<button class="prev">&laquo;</button><button class="next">&raquo;</button> <span>Съвпадение <strong>1</strong> от ' + res.length + '</span>')
                            .show();
                        res_o = [];
                        res.each(function () {
                            res_o.push($(this).offset().top);
                        });
                        current = false;
                        window.scrollTo(0, window.scrollY - 80);
                        $('.menu-search .next').click();
                    } else {
                        res_o = [];
                        current = false;
                        $('.menu-search')
                            .text('Няма резултати')
                            .show();
                    }
                }
            } else {
                last_q = null;
                $('.search-result').removeClass('search-result');
                $('.menu-search').text('').hide();
            }
        }, 300);
    });
});