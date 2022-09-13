using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreedPassMiner
{
    internal static class Log
    {
        static readonly object locker = new();

        public static void LogError(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("*************异常文本*************");
            sb.AppendLine("【出现时间】：" + DateTime.Now.ToString());
            if (e != null)
            {
                sb.AppendLine("【异常类型】：" + (e.GetType()?.Name ?? "未知"));
                sb.AppendLine("【异常信息】：" + e.Message);
                sb.AppendLine("【异常方法】：" + e.TargetSite);
                sb.AppendLine("【堆栈调用】：" + e.StackTrace);

                if (e.InnerException != null)
                {
                    sb.AppendLine("【InnerException】：");
                    sb.AppendLine("【异常类型】：" + (e.InnerException.GetType()?.Name ?? "未知"));
                    sb.AppendLine("【异常信息】：" + e.InnerException.Message);
                    sb.AppendLine("【异常方法】：" + e.InnerException.TargetSite);
                    sb.AppendLine("【堆栈调用】：" + e.InnerException.StackTrace);
                }
            }
            else
            {
                sb.AppendLine("【空异常】：");
            }
            sb.AppendLine("**********************************");

            lock (locker)
            {
                try { File.AppendAllTextAsync("Exception.txt", sb.ToString()); }
                catch { }
            }
        }
    }
}
