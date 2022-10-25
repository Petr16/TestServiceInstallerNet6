using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NLog.Web;

namespace TestServiceInstallerNet6;

public static class Program
{
    private static ProcessModule GetMainModule()
    {
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule == null)
        {
            throw new Exception("Failed to get main module");
        }

        return processModule;
    }
    
    private static IConfiguration LoadConfiguration()
    {
        var processModule = GetMainModule();
        var pathToExe = processModule.FileName;
        var pathToContentRoot = Path.GetDirectoryName(pathToExe);

        var builder = new ConfigurationBuilder()
                // .SetBasePath(Directory.GetCurrentDirectory())
                .SetBasePath(pathToContentRoot)
                //.AddYaml("conf/config.yml")
                .AddJsonFile("config.json")
            ;
        return builder.Build();
    }
    
    static void Main()
    {
        var processModule = GetMainModule();
        var pathToExe = processModule.FileName;
        var pathToFolder = Path.GetDirectoryName(pathToExe);
        var logger = NLogBuilder.ConfigureNLog(Path.Combine(pathToFolder, "conf", "nlog.config"))
            .GetCurrentClassLogger();
        //var logger = NLogBuilder.ConfigureNLog("conf/nlog.config").GetCurrentClassLogger();
        var configuration = LoadConfiguration();
        logger.Info("Start");
        configuration["paths:base"] = pathToFolder;
        try
        {
            var installer = new Installer(configuration, logger);
            installer.Install();
        }
        catch (Exception e)
        {
            logger.Error(e.Message);
            Console.WriteLine(e);
            throw;
        }
    }
}