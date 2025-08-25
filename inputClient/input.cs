using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleRpc.Transports.Http.Client;
using GasContract;
using NLog;
using SimpleRpc.Transports;
using SimpleRpc.Serialization.Hyperion;

namespace InputClient
{
    /// <summary>
    /// The InputClient class handles interaction with the gas container service by increasing the gas mass when pressure is below a specified limit.
    /// </summary>
    class InputClient
    {
        /// <summary>
        /// Logger for logging client events.
        /// </summary>
        private Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Random generator for generating random mass values to add to the gas container.
        /// </summary>
        private Random rnd = new Random();

        /// <summary>
        /// Configures the logging system using NLog.
        /// </summary>
        private void ConfigureLogging()
        {
            // Create a new NLog configuration
            var config = new NLog.Config.LoggingConfiguration();

            // Set up a console target that writes log messages to the console
            var console = new NLog.Targets.ConsoleTarget("console")
            {
                // Define the layout (format) of the log messages
                Layout = @"${date:format=HH\:mm\:ss}|${level}| ${message} ${exception}"
            };

            // Add the console target to the logging configuration
            config.AddTarget(console);

            // Apply logging rules to log all levels of messages (Info, Warn, Error, etc.)
            config.AddRuleForAllLevels(console);

            // Apply the logging configuration
            LogManager.Configuration = config;
        }

        /// <summary>
        /// Sets up the SimpleRPC client and continuously checks gas pressure to add mass when appropriate.
        /// </summary>
        private void Run()
        {
            // Configure the logging system
            ConfigureLogging();
            mLog.Info("Starting Input Client...");

            // Set up dependency injection container for RPC services
            var sc = new ServiceCollection();

            // Add SimpleRPC client configuration, specifying the URL and serializer for RPC communication
            sc.AddSimpleRpcClient(
                "gasService", // Name of the service
                new HttpClientTransportOptions
                {
                    // URL of the RPC server (ensure this matches the server configuration)
                    Url = "http://127.0.0.1:5001/gasrpc",
                    Serializer = "HyperionMessageSerializer" // Using Hyperion for serialization
                }
            )
            // Add Hyperion serializer to the service collection
            .AddSimpleRpcHyperionSerializer();

            // Register the GasContainerService proxy for remote procedure calls (RPC)
            sc.AddSimpleRpcProxy<IGasContainerService>("gasService");

            // Build the service provider for dependency injection
            var serviceProvider = sc.BuildServiceProvider();

            // Get the service proxy for the gas container service
            var gasService = serviceProvider.GetService<IGasContainerService>();

            // If the gas service could not be retrieved, log an error and exit
            if (gasService == null)
            {
                mLog.Error("Failed to retrieve the GasContainerService. Ensure the server is running and accessible.");
                return; // Exit if the service is unavailable
            }

            mLog.Info("Successfully connected to GasContainerService.");

            // Main loop: Continuously attempt to increase mass
            while (true)
            {
                try
                {
                    // Check if the container is destroyed
                    if (gasService.IsDestroyed())
                    {
                        mLog.Info("The container has been destroyed. Stopping updates.");
                        break; // Exit the loop if the container is destroyed
                    }

                    // Get current pressure from the gas container
                    double currentPressure = gasService.GetPressure();
                    mLog.Info($"Current pressure: {currentPressure}");

                    // Add mass if the pressure is below the lower limit (150)
                    if (currentPressure < 150)
                    {
                        // Randomly generate a mass to add (between 1 and 4 units)
                        int massToAdd = rnd.Next(1, 5);

                        // Call the IncreaseMass method on the remote gas service
                        gasService.IncreaseMass(massToAdd);
                        mLog.Info($"Added {massToAdd} units of mass.");
                    }
                    else
                    {
                        // Log that no mass was added because the pressure exceeded the threshold
                        mLog.Info("Pressure is above the threshold, no mass added.");
                    }

                    // Sleep for 2 seconds before the next operation
                    Thread.Sleep(2000);
                }
                catch (HttpRequestException e)
                {
                    // Log connection-related exceptions (like network issues) and retry after a delay
                    mLog.Warn(e, "Unable to connect to server. Retrying...");
                    Thread.Sleep(5000); // Wait longer before retrying to avoid excessive requests
                }
                catch (Exception e)
                {
                    // Log any other unhandled exceptions and continue the loop
                    mLog.Warn(e, "Unhandled exception caught. Retrying...");
                    Thread.Sleep(2000); // Wait before retrying to avoid spamming
                }
            }
        }

        /// <summary>
        /// Entry point for the InputClient program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            // Create an instance of InputClient and start the Run method
            var self = new InputClient();
            self.Run();
        }
    }
}
