using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace JinRi.LogCenter.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //HostFactory.Run(x =>
            //{
            //    x.Service<TownCrier>(s =>
            //    {
            //        s.ConstructUsing(name => new TownCrier());
            //        s.WhenStarted(tc => tc.Start());
            //        s.WhenStopped(tc => tc.Stop());
            //    });
            //    x.RunAsLocalSystem();

            //    x.SetDescription("Sample Topshelf Host");
            //    x.SetDisplayName("Stuff");
            //    x.SetServiceName("Stuff");
            //});

            HostFactory.Run(x =>
            {
                x.Service<IService>(s =>
                    {
                        s.ConstructUsing(name => new LoggerDataServeice());
                        s.WhenStarted(tc => tc.Start());
                        s.WhenStopped(tc => tc.Stop());
                    });
                x.RunAsLocalService();
                x.SetServiceName("JinRi.LogCenter");
                x.SetDisplayName("JinRi.LogCenter日志中心服务");
                x.SetDescription("JinRi.LogCenter日志中心服务");
            });
           
            
        }
    }
}
