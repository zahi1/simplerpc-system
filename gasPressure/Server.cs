using System.Net;
using NLog;
using GasContract; // Updated to use GasContract
using SimpleRpc.Transports;
using SimpleRpc.Transports.Http.Server;
using SimpleRpc.Serialization.Hyperion;
using Microsoft.Extensions.DependencyInjection;

namespace GasPressure // Changed namespace to match your project
{
    /// <summary>
    /// This class represents the server that provides the gas container service.
    /// It handles configuration of logging, server setup, and RPC services to communicate with clients.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Logger for logging server events and messages.
        /// </summary>
        Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures the logging subsystem using NLog.
        /// </summary>
        private void ConfigureLogging()
        {
            // Create a new logging configuration
            var config = new NLog.Config.LoggingConfiguration();

            // Set up a console target that writes log messages to the console
            var console = new NLog.Targets.ConsoleTarget("console")
            {
                // Define the layout (format) of the log messages
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
        /// Entry point of the program.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            // Create a new instance of the Server class
            var self = new Server();
            // Call the Run method to start the server
            self.Run(args);
        }

        /// <summary>
        /// Main body of the program.
        /// This method configures logging and starts the server.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void Run(string[] args)
        {
            // Configure logging system
            ConfigureLogging();

            // Log that the server is about to start
            log.Info("Server is about to start");

            // Start the server
            StartServer(args);
        }

        /// <summary>
        /// Starts the integrated server and sets up SimpleRPC services and middleware.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        private void StartServer(string[] args)
        {
            // Create a web application builder using the provided command line arguments
            var builder = WebApplication.CreateBuilder(args);

            // Configure the integrated server to listen on localhost at port 5001
            builder.WebHost.ConfigureKestrel(opts =>
            {
                opts.Listen(IPAddress.Loopback, 5001);
            });

            // Add SimpleRPC services with HTTP transport and Hyperion serialization
            builder.Services
                .AddSimpleRpcServer(new HttpServerTransportOptions { Path = "/gasrpc" })
                .AddSimpleRpcHyperionSerializer();

            // Add custom gas container service as a singleton
            builder.Services
                .AddSingleton<IGasContainerService>(new GasContainerService());

            // Build the server application
            var app = builder.Build();

            // Add SimpleRPC middleware to handle RPC requests
            app.UseSimpleRpcServer();

            // Run the server to start listening for requests
            app.Run();
            // app.RunAsync(); // Uncomment this line if background processing is needed
        }
    }
}
