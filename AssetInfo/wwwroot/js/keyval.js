((win) => {
    // help
    let post = win.ns.post;
    let get = win.ns.get;
    let cfg = win.ns.cfg;
    let page = win.ns.page;

    list();
    // table data
    function list() {
        let ctgy = $('#keyvalCtgy').val();
        post(cfg.ApiKVList, { category: ctgy })
            .then(data => {
                if (data.errcode == 200) {
                    createTable(data.list);
                } else if (data.errmsg) {
                    $('#keyvallist').html(`<p class="text-warning">${data.errmsg}</p>`);
                } else
                    throw new Error(data);
            })
            .catch(err => {
                $('#keyvallist').html(`<p class="text-danger">${err.message}</p>`);
            });
    }
    function createTable(data) {
        // table dom
        let th = '<tr><th>#</th><th>名称</th><th>说明</th><th>功能</th></tr>';
        let rows = '';
        for (var i = 0, len = data.length; i < len; i++) {
            let item = data[i];
            let ischecked = item.Enabled == 1 ? "checked" : '';
            let onoff = `<input class="onoff-check" id="${item.Code}" type="checkbox" ${ischecked}/><label class="onoff-label" for="${item.Code}" ></label>`;
            let cols = `<td>${item.Code}</td><td>${item.Title}</td><td>${item.Comment}</td>`;
            cols += `<td>${onoff}<span class="btn sm info mg-lr-5" act="up" kvcode="${item.Code}">↑</span>`;
            cols += `<span class="btn sm success" act="down" kvcode="${item.Code}">↓</span>`;
            cols += ` <a class="btn info sm update-btns" kvcode="${item.Code}" >更新</a></td>`;
            rows += `<tr kvcode="${item.Code}">${cols}</tr>`;
        }
        let table = `<table class="table hover">${th}${rows}</table>`;
        $('#keyvallist').empty().html(table);
        //
        opBind();
    }

    // 操作事件绑定
    function opBind() {
        // on off事件
        $('#keyvallist .onoff-label').each(item => {
            item.onclick = () => {
                let ischeck = $(item).prev()[0].checked;
                post(cfg.ApiKVOnOff, { tableid: 'keyval', colid: $(item).prop('for'), onoff: ischeck ? 0 : 1 });
            };
        });
        // 排序事件
        $('#keyvallist [act=up]').click(item => {
            let kvcode = $(item).prop('kvcode');
            orderby(kvcode, 0);
        });
        $('#keyvallist [act=down]').click(item => {
            let kvcode = $(item).prop('kvcode');
            orderby(kvcode, 1);
        });
        // 更新
        $('#keyvallist .update-btns').click(item => {
            let kvcode = $(item).prop('kvcode');
            update(kvcode);
        });
    }

    // 排序操作 kvcode:要重新排序的kvcode,dir:0=向前,1=向后
    function orderby(kvcode, dir) {
        // 当前排序顺序
        let order = [];
        $('#keyvallist tr[kvcode]').each(item => {
            order.push($(item).prop('kvcode'));
        });
        // kvcode当前位置
        let index = order.indexOf(kvcode);
        // 排序
        if (dir == 0 && index > 0) {
            let prevTmp = order[index - 1];
            order[index - 1] = kvcode;
            order[index] = prevTmp;
        } else if (dir == 1 && index < order.length - 1) {
            let nextTmp = order[index + 1];
            order[index + 1] = kvcode;
            order[index] = nextTmp;
        }
        // 修改数据库排序字段
        post(cfg.ApiKVOrderBy, { orderby: order.join(',') })
            .then(data => {
                if (data.errcode == 200) {
                    // 刷新表格
                    list();
                } else if (data.errmsg) {
                    msgbox.alert(data.errmsg);
                } else
                    throw new Error(data);
            })
            .catch(err => {
                msgbox.alert(err.message);
            });
    }

    // 增加
    $('#keyvaladdBtn').click(thisBtn => {
        add(thisBtn);
    });
    function add(thisobj) {
        $('#errinfobox').html('');
        // valdate
        let inputs = $('#keyvalform input[name]');
        for (var i = 0, len = inputs.length; i < len; i++) {
            if (!$.formCheck(inputs[i]))
                return;
        }
        $ui.isBtnLoading(thisobj);
        //
        let para = $.formJson($('#keyvalform')[0]);
        post(cfg.ApiKVAdd, para)
            .then(data => {
                if (data.errcode == 200) {
                    $('#errinfobox').html('成功!');
                    $('#keyvalform input[type=text]').val('');
                    // 刷新表格
                    list();
                } else if (data.errmsg) {
                    $('#errinfobox').html(data.errmsg);
                } else
                    throw new Error(data);
                $ui.clsBtnLoading(thisobj, 500);
            })
            .catch(err => {
                $('#errinfobox').html(err.message);
                $ui.clsBtnLoading(thisobj, 500);
            });
    }

    // 修改
    function update(code) {
        // 获取数据,填写在新增表单
        post(cfg.ApiKVItem, { code: code })
            .then(data => {
                if (data.errcode == 200) {
                    // 填充表单
                    $('#keyvalform input[name]').each(o => {
                        o.value = data.item[o.name];
                    });
                } else if (data.errmsg) {
                    $('#errinfobox').html('更新出错: '+data.errmsg);
                } else
                    throw new Error(data);
            })
            .catch(err => {
                $('#errinfobox').html('更新发生异常: '+err.message);
            });
    }
})(window);