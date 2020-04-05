using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace AssetInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            // 主机功能配置项
            void serverCfg(IServiceCollection service)
            {
                // 跨域功能
                service.AddCors();
            }

            // 跨域策略配置项
            void cors(CorsPolicyBuilder cfg)
            {
                cfg.AllowAnyHeader();
                cfg.AllowAnyMethod();
                cfg.AllowAnyOrigin();
                cfg.AllowCredentials();
            }

            // 默认文档
            DefaultFilesOptions defaultCfg = new DefaultFilesOptions();
            defaultCfg.DefaultFileNames.Add("asset.html");

            // kestrel服务器配置文件载入
            IConfiguration kestrelCfg = new ConfigurationBuilder()
                .AddJsonFile("kestrel.json")
                .Build();

            // 开机运行,可选择其中一种方式运行,服务或者控制台
            // 实例化主机,载入配置项
            IWebHost webhost = new WebHostBuilder()
                .ConfigureServices(serverCfg)
                .UseConfiguration(kestrelCfg)
                .UseKestrel()
                .Configure(app => app
                    // 默认文档和静态文件
                    .UseDefaultFiles(defaultCfg)
                    .UseStaticFiles()
                    // 跨域
                    .UseCors(cors)
                    // 能在请求页面上显示异常信息
                    //.UseDeveloperExceptionPage()
                    // 自定义异常处理返回中间件
                    .UseExceptionHandler(ApiHandler.CustomExceptionHandlerOptions())
                    .Use(ApiHandler.UrlHandler)// 自定义路由中间件
                )
                .Build();

            // 打开浏览器,打开前端网页
            string url = kestrelCfg.GetSection("server.urls").Value.Trim(';');
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                Process p = Process.Start("xdg-open", url);
                p.Dispose();
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                Process p = Process.Start("open", url);
                p.Dispose();
            }
            else
            {
                Process p = Process.Start(
                    new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                        Arguments = url
                    });
                p.Dispose();
            }
            // process打开浏览器后,不会马上释放浏览器进程.此时再打开新的浏览器时会打不开.用gc.collect强制回收后才可以打开
            GC.Collect();
            //
            webhost.Run();
        }
    }
}
