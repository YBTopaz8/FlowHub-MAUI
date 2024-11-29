


namespace FlowHub_MAUI
{
    public partial class App : Application
    {
        public FlowHubWindow FlowHubWindow { get; }
        public App(FlowHubWindow flowHubWindow)
        {
            InitializeComponent();

            InitializeParseClient();

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            FlowHubWindow = flowHubWindow;
        }

        private void CurrentDomain_FirstChanceException(object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            Debug.WriteLine($"********** UNHANDLED EXCEPTION! Details: {e.Exception} | {e.Exception.InnerException?.Message} | {e.Exception.Source} " +
                $"| {e.Exception.StackTrace} | {e.Exception.Message} || {e.Exception.Data.Values} {e.Exception.HelpLink}");

            //var home = IPlatformApplication.Current!.Services.GetService<HomePageVM>();
            //await home.ExitingApp();
            LogException(e.Exception);
        }


        protected override Window CreateWindow(IActivationState? activationState)
        {

            var vm = IPlatformApplication.Current!.Services.GetService<HomePageVM>()!;
#if WINDOWS
        FlowHubWindow.Page = new AppShell(vm);

#elif ANDROID
            FlowHubWindow.Page = new AppShellMobile();
#endif

            //win = base.CreateWindow(activationState);
            //this.MinimumHeight = 800;
            //this.MinimumWidth = 1200;
            //this.Height = 900;
            //this.Width = 1200;

            return FlowHubWindow;
        }


        public static bool InitializeParseClient()
        {
            try
            {
                // Check for internet connection
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    Console.WriteLine("No Internet Connection: Unable to initialize ParseClient.");
                    return false;
                }

                // Validate API Keys
                if (string.IsNullOrEmpty(APIKeys.ApplicationId) ||
                    string.IsNullOrEmpty(APIKeys.ServerUri) ||
                    string.IsNullOrEmpty(APIKeys.DotNetKEY))
                {
                    Console.WriteLine("Invalid API Keys: Unable to initialize ParseClient.");
                    return false;
                }

                // Create ParseClient
                ParseClient client = new ParseClient(new ServerConnectionData
                {
                    ApplicationID = APIKeys.ApplicationId,
                    ServerURI = APIKeys.ServerUri,
                    Key = APIKeys.DotNetKEY,
                }
                );

                HostManifestData manifest = new HostManifestData()
                {
                    Version = "1.0.0",
                    Identifier = "com.yvanbrunel.dimmer",
                    Name = "Dimmer",
                };

                client.Publicize();


                Console.WriteLine("ParseClient initialized successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ParseClient: {ex.Message}");
                return false;
            }
        }
        private static readonly object _logLock = new object();


        private void LogException(Exception ex)
        {

            try
            {
                // Define the directory path
                string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DimmerCrashLogs");

                // Ensure the directory exists; if not, create it
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                string filePath = Path.Combine(directoryPath, "crashlog.txt");
                string logContent = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\nMsg:{ex.Message}\nStackTrace:{ex.StackTrace}\n\n";

                // Retry mechanism for file writing
                bool success = false;
                int retries = 3;
                int delay = 500; // Delay between retries in milliseconds

                lock (_logLock)
                {
                    while (retries-- > 0 && !success)
                    {
                        try
                        {
#if RELEASE || DEBUG
                            File.AppendAllText(filePath, logContent);
                            success = true; // Write successful
#endif
                        }
                        catch (IOException ioEx) when (retries > 0)
                        {
                            Debug.WriteLine($"Failed to log, retrying... ({ioEx.Message})");
                            Thread.Sleep(delay); // Wait and retry
                        }
                    }

                    if (!success)
                    {
                        Debug.WriteLine("Failed to log exception after multiple attempts.");
                    }
                }
            }
            catch (Exception loggingEx)
            {
                Debug.WriteLine($"Failed to log exception: {loggingEx}");
            }
        }
    }


}