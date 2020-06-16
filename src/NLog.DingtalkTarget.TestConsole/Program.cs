using System;

namespace NLog.DingtalkTarget.TestConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();

            logger.Warn("NLog.DingtalkTarget Test Warn");
            logger.Error("NLog.DingtalkTarget Test Error");

            Console.Read();
        }
    }
}