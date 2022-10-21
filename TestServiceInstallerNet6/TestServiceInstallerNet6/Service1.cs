using System.ServiceProcess;

namespace TestServiceInstallerNet6;

public partial class Service1: ServiceBase
{
    Logger logger;
    public Service1()
    {
        InitializeComponent();
        //https://metanit.com/sharp/tutorial/11.1.php
        this.CanStop = true; // службу можно остановить
        this.CanPauseAndContinue = true; // службу можно приостановить и затем продолжить
        this.AutoLog = true; // служба может вести запись в лог
    }

    protected override void OnStart(string[] args)
    {
        logger = new Logger();
        Thread loggerThread = new Thread(new ThreadStart(logger.Start));
        loggerThread.Start();
    }

    protected override void OnStop()
    {
        logger.Stop();
        Thread.Sleep(1000);
    }
}