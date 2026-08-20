using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Quartz;
using log4net;

namespace Lvy.Trip.Weixin.Job
{
    public class MyJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(MyJob));

        public void Execute(IJobExecutionContext context)
        {
            logger.Info("MyJob running...");


            logger.Info("MyJob run finished.");
        }
    }

}