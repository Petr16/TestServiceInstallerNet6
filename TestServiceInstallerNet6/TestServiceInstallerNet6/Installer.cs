using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Extensions.Configuration;

namespace TestServiceInstallerNet6;
public class Parameters
{
    public Paths Paths { get; set; }
}

public class Paths
{
    public string Base { get; set; }
}

public class Installer
{
    private readonly NLog.Logger logger_;
    private readonly Parameters parameters_;
    
    public Installer(IConfiguration _configuration, NLog.Logger _logger)
    {
        logger_ = _logger;
        
        parameters_ = new Parameters()
        {
            Paths = new Paths()
            {
                Base = _configuration.GetValue<string>("paths:base"),
            }
        };
    }
    public void Install()
    {
        try
        {
            var services = new Dictionary<string, string>
            {
                ["TestServiceInstallerNet6"] = Path.Combine(parameters_.Paths.Base, "install.bat"),

            };
            InstallServices(services);
            logger_.Info("Success");
        }
        catch (Exception e)
        {
            //Helper.LogException(e);
            logger_.Error(e.Message);
            throw;
        }
        finally
        {
            //File.Delete(scriptDbPath);
        }
    }
    
    private void InstallServices(IDictionary<string, string> _services)
    {
        foreach (var kv in _services)
        {
            var serviceName = kv.Key;
            var installerPath = kv.Value;
            logger_.Info($"Installing service {serviceName}");
            try
            {
                StopService(serviceName);
                //ExecuteFileWithArguments("cmd.exe", $"sc stop {serviceName}");
                ExecuteFileWithArguments(installerPath);
                StartService(serviceName);
                //ExecuteFileWithArguments("cmd.exe", $"sc start {serviceName}");

                const int resetAfter = 60000;
                //ExecuteFileWithArguments("cmd.exe", $"C:/Windows/Microsoft.NET/Framework64/v4.0.30319/InstallUtil.exe {}");
            }
            catch (Exception e)
            {
                //Helper.LogException(e);
                throw new Exception($"Failed to install {serviceName} service", e);
            }
        }
    }
    
    private void StopService(string _serviceName)
        {
            // shutdown service
            try
            {
                var service = new ServiceController(_serviceName);

                if (service.Status != ServiceControllerStatus.Stopped &&
                    service.Status != ServiceControllerStatus.StopPending)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }
            catch (Exception e)
            {
                logger_.Error(e.Message);
                throw;
            }
        }

        private void StartService(string _serviceName)
        {
            try
            {
                //logger_.Debug($"Start service {_serviceName}");
                var service = new ServiceController(_serviceName);

                if (service.Status != ServiceControllerStatus.Running &&
                    service.Status != ServiceControllerStatus.StartPending)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(15));
                }
            }
            catch (Exception e)
            {
                logger_.Error(e.Message);
                throw;
            }
        }

        private void ExecuteFileWithArguments(string _filePath, string _arguments = null)
        {
            var procData = new ProcessStartInfo();
            procData.FileName = _filePath;
            if (!(_arguments is null))
            {
                procData.Arguments = _arguments;
            }
            procData.WindowStyle = ProcessWindowStyle.Hidden;
            procData.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = procData;
            process.Start();
            process.WaitForExit();

            if (process.ExitCode == -1)
            {
                var message = process.StandardOutput.ReadToEnd();
                logger_.Error(message);
                throw new Exception(process.StandardOutput.ReadToEnd());
            }
        }
}