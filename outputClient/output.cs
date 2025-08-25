using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Transports.Http.Client;
using GasContract;
using NLog;
using SimpleRpc.Transports;
using SimpleRpc.Serialization.Hyperion;

namespace OutputClient
{
    /// <summary>
    /// The OutputClient class handles interaction with the gas container service by removing gas mass when the pressure is above a specified threshold.
    /// </summary>
    class OutputClient
    {
        /// <summary>
        /// Logger to record information, warnings, and errors.
        /// </summary>
        private Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Random generator to simulate the random removal of gas mass.
        /// </summary>
        private Random rnd = new Random();

        /// <summary>
        /// Configures the logging system using NLog.
        /// </summary>
        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Setting up the console output for logs
            var console = new NLog.Targets.ConsoleTarget("console")
            {
                Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
            };

            // Add the console target to the logging configuration
            config.AddTarget(console);

            // Configure logging for all levels (Info, Warning, Error, etc.)
            config.AddRuleForAllLevels(console);

            // Apply the logging configuration globally
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Main logic of the OutputClient. Continuously communicates with the server to remove gas mass when pressure exceeds a certain threshold.
        /// </summary>
        private void Run()
        {
            // Configure the logging system
            ConfigureLogging();
            mLog.Info("Starting Output Client...");

            // Set up the RPC client to communicate with the gas container service
            var sc = new ServiceCollection();

            // Add SimpleRPC client with the necessary configurations
            sc.AddSimpleRpcClient(
                "gasService", // Service name
                new HttpClientTransportOptions
                {
                    Url = "http://127.0.0.1:5001/gasrpc", // URL of the gas container service
                    Serializer = "HyperionMessageSerializer" // Hyperion serializer for serialization
                }
            )
            .AddSimpleRpcHyperionSerializer(); // Add Hyperion serialization support

            // Register the GasContainerService proxy for RPC
            sc.AddSimpleRpcProxy<IGasContainerService>("gasService");

            // Build the service provider
            var serviceProvider = sc.BuildServiceProvider();

            // Get the GasContainerService proxy to interact with the server
            var gasService = serviceProvider.GetService<IGasContainerService>();

            // If the gas service is not found, log an error and exit
            if (gasService == null)
            {
                mLog.Error("Unable to connect to GasContainerService.");
                return;
            }

            mLog.Info("Successfully connected to GasContainerService.");

            // Continuously run the client operations
            while (true)
            {
                try
                {
                    // Check if the container is destroyed on the server side
                    if (!gasService.IsDestroyed())
                    {
                        // Get the current pressure from the gas container
                        double pressure = gasService.GetPressure();
                        mLog.Info($"Current pressure: {pressure}");

                        // If the pressure is above 150, remove some gas mass
                        if (pressure > 150)
                        {
                            // Randomly choose how much mass to remove (1-4 units)
                            int massToRemove = rnd.Next(1, 5);

                            // Decrease the mass on the server by calling the RPC method
                            gasService.DecreaseMass(massToRemove);
                            mLog.Info($"Removed {massToRemove} units of mass.");
                        }
                        else
                        {
                            // Log if the pressure is too low to remove mass
                            mLog.Info("Pressure below the threshold for removing mass.");
                        }

                        // Sleep for 2 seconds before the next operation
                        Thread.Sleep(2000);
                    }
                    else
                    {
                        // If the container is destroyed, stop further operations
                        mLog.Info("The container has been destroyed. Stopping updates.");
                        break; // Exit the loop
                    }
                }
                catch (Exception e)
                {
                    // Log any exceptions (connection issues, etc.) and retry after a delay
                    mLog.Warn(e, "Unable to connect to server. Retrying...");

                    // Wait 2 seconds before retrying
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// Entry point for the OutputClient application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            // Create an instance of OutputClient and run its main logic
            var self = new OutputClient();
            self.Run();
        }
    }
}
