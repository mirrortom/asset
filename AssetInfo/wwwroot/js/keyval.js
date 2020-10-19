((win) => {
    // help
    let post = win.ns.post;
    let get = win.ns.get;
    let cfg = win.ns.cfg;
    let page = win.ns.page;
    let msg = win.msgshow('#msginfobox');

    //
    chgTitle();
    list();
    // table data
    function list() {
        let ctgy = $('#keyvalCtgy').val();
        post(cfg.ApiKVList, { category: ctgy })
            .then(data => {
                $('#keyvallist').empty();
                if (data.errcode == 200) {
                    createTable(data.list);
                } else if (data.errmsg) {
                    msg.info(data.errmsg)
                } else
                    throw new Error('服务器异常!');
            })
            .catch(err => {
                msg.err(err.message);
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
        msg.clear();
        // valdate
        let para = formCheck();
        if (para == false)
            return;
        if ($ui.isBtnLoading(thisobj)) {
            return;
        }
        //
        post(cfg.ApiKVAdd, para)
            .then(data => {
                if (data.errcode == 200) {
                    msg.ok('添加完成.');
                    resetForm();
                    // 刷新表格
                    list();
                } else if (data.errmsg) {
                    msg.err(data.errmsg);
                } else
                    throw new Error(data);
                $ui.clsBtnLoading(thisobj, 500);
            })
            .catch(err => {
                $('#msginfobox').html(err.message);
                $ui.clsBtnLoading(thisobj, 500);
            });
    }

    function formCheck() {
        let inputs = $('#keyvalform input[name]');
        for (var i = 0, len = inputs.length; i < len; i++) {
            if (!$.formCheck(inputs[i]))
                return false;
        }
        if (!$('#keyvalCtgy').val()) {
            msg.warn('选项分组id错误!');
            return false;
        }
        let para = $.formJson($('#keyvalform')[0]);
        return para;
    }
    // 取消
    $('#keyvalclearBtn').click(thisBtn => {
        resetForm();
    });

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
                    chgTitle(1);
                } else if (data.errmsg) {
                    msg.err('更新出错: ' + data.errmsg)
                } else
                    throw new Error('服务器异常!');
            })
            .catch(err => {
                $('#msginfobox').html('更新发生异常: ' + err.message);
            });
    }
    // 重置表单
    function resetForm() {
        chgTitle();
        $('#keyvalform input[name=Title]').val('');
        $('#keyvalform input[name=Comment]').val('');
        $('#keyvalform input[name=Code]').val('');
    }
    // 新增/修改 标题切换 1=修改,其它=新增
    function chgTitle(type) {
        if (type == 1) {
            $('#keyvalTitle').text('🖍 更新').addClass('text-danger');
        } else {
            $('#keyvalTitle').text('\u271A 新增').removeClass('text-danger');
        }
    }
})(window);