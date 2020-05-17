((win) => {
    //=======================
    // window对象上的命名空间.
    //=======================
    let ns = {};

    //=============================================
    // 项目命名空间:页面脚本封闭函数中需要提供到外部使用的,
    // 对象,变量,函数,页面间传值等,统一绑定在此对象
    //=============================================
    ns.page = {};

    //===========================================
    // cfg:公用配置对象. 方法小写开头,属性大写开头
    //===========================================
    let cfg = {};
    // api host port
    cfg.apiUrl = (path) => { return 'http://localhost:25800' + path; };
    // api-url
    // --login
    cfg.ApiLogin = cfg.apiUrl('/account/login');
    // --keyval
    cfg.ApiKVAdd = cfg.apiUrl('/keyval/add');
    cfg.ApiKVItem = cfg.apiUrl('/keyval/item');
    cfg.ApiKVList = cfg.apiUrl('/keyval/list');
    cfg.ApiKVOnOff = cfg.apiUrl('/keyval/onoff');
    cfg.ApiKVOrderBy = cfg.apiUrl('/keyval/orderby');
    // --asset
    cfg.ApiAssetAdd = cfg.apiUrl('/asset/add');
    cfg.ApiAssetList = cfg.apiUrl('/asset/list');
    cfg.ApiAssetItem = cfg.apiUrl('/asset/item');
    cfg.ApiAssetHistory = cfg.apiUrl('/asset/history');
    cfg.ApiAssetStatistic = cfg.apiUrl('/asset/statistic');
    cfg.ApiAssetTitles = cfg.apiUrl('/asset/titles');
    // --
    ns.cfg = cfg;

    //==================
    // token 存取
    //==================
    let token = {};
    // 存
    token.newToken = (token) => {
        let now = new Date();
        // 预计2小时过期时间
        let expire = new Date(now.setHours(now.getHours() + 2));
        let tk = { token: token, expire: $.datefmt(expire) };
        //
        localStorage.setItem('token', JSON.stringify(tk));
    };
    // 取
    token.get = () => {
        let tkjson = localStorage.getItem('token');
        if (!tkjson) return '';
        let tk = JSON.parse(tkjson);
        return tk.token;
    };
    // 删除
    token.del = () => {
        localStorage.removeItem('token');
    };
    // 检查过期
    token.check = () => {
        let tkjson = localStorage.getItem('token');
        if (!tkjson) return false;
        let tk = JSON.parse(tkjson);
        if (!tk.expire) return false;
        //console.log(tk);
        return $.dateByfmt(tk.expire) > new Date();
    };
    // --
    ns.token = token;

    //==================
    // 公用带token的ajax
    //==================
    ns.get = (url, para, resType = 'html') => {
        let initCfg = { headers: { 'Auth': ns.token.get() } };
        return $.get(url, para, initCfg, resType);
    };
    ns.post = (url, para, resType = 'json') => {
        let initCfg = { headers: { 'Auth': ns.token.get()  } };
        return $.post(url, para, initCfg, resType);
    };

    //==================
    // 主菜单路由
    //==================
    let router = {};
    // 当前活动菜单路径
    router.url = cfg.apiUrl('/');
    // 所有url
    router.urls = null;
    /**
     * 路由跳转url.一个函数,自定义实现
     * @param {string} url 菜单url
     * @param {any} para 页面传递参数.默认null
     */
    router.goto = (url, para = null) => { };
    // 路由参数 {any}
    router.para = null;
    // --
    ns.router = router;

    // 引用名称
    win.ns = ns;
})(window);