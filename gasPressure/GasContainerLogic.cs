using System;
using System.Threading;
using NLog;

namespace GasPressure
{
    /// <summary>
    /// This class holds the state of the gas container, including temperature, mass, and pressure.
    /// It also provides the methods for calculating pressure and resetting the container.
    /// </summary>
    public class GasContainerState
    {
        /// <summary>
        /// Object used for thread safety to lock the state when accessed by multiple threads.
        /// </summary>
        public readonly object AccessLock = new object();

        /// <summary>
        /// Temperature of the gas in Kelvin (initial value is room temperature 293K).
        /// </summary>
        public double Temperature { get; set; } = 293;

        /// <summary>
        /// Mass of the gas in arbitrary units (initial value is 10 units).
        /// </summary>
        public double Mass { get; set; } = 10;

        /// <summary>
        /// Indicates whether the gas container is destroyed (by implosion or explosion).
        /// </summary>
        public bool IsDestroyed { get; set; } = false;

        /// <summary>
        /// The pressure limit at which input components stop adding mass (default 110 units).
        /// </summary>
        public double PressureLimit { get; set; } = 110;

        /// <summary>
        /// The upper pressure limit at which output components can start removing mass (default 125 units).
        /// </summary>
        public double UpperPressureLimit { get; set; } = 125;

        /// <summary>
        /// The pressure limit above which the container will explode (default 140 units).
        /// </summary>
        public double ExplosionLimit { get; set; } = 140;

        /// <summary>
        /// The pressure limit below which the container will implode (default 40 units).
        /// </summary>
        public double ImplosionLimit { get; set; } = 40;

        /// <summary>
        /// Calculates the pressure based on the mass and temperature of the gas.
        /// </summary>
        /// <returns>The calculated pressure using an ideal gas law approximation (P = (m * T) / V).</returns>
        public double Pressure
        {
            get
            {
                // Pressure is derived from mass and temperature (constant V = 22.4)
                return (Mass * Temperature) / 22.4;
            }
        }

        /// <summary>
        /// Resets the gas container state after destruction, bringing it back to its initial state.
        /// </summary>
        public void Reset()
        {
            Mass = 10; // Resetting the mass to the initial value.
            Temperature = 293; // Resetting the temperature to the initial value (room temperature).
            IsDestroyed = false; // Mark the container as not destroyed.
        }
    }

    /// <summary>
    /// Contains the business logic for managing the gas container's state,
    /// including multithreading, pressure management, and temperature adjustments.
    /// </summary>
    public class GasContainerLogic
    {
        /// <summary>
        /// Instance of the GasContainerState class, which holds the current state of the gas container.
        /// </summary>
        private readonly GasContainerState state = new GasContainerState();

        /// <summary>
        /// NLog logger instance used to record log information.
        /// </summary>
        private Logger mLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Background thread that handles temperature adjustments and periodic pressure checks.
        /// </summary>
        private Thread mBackgroundThread;

        /// <summary>
        /// Constructor that initializes the logic and starts the background thread for temperature adjustments.
        /// </summary>
        public GasContainerLogic()
        {
            // Start a new background thread that will run the BackgroundTask method.
            mBackgroundThread = new Thread(BackgroundTask);
            mBackgroundThread.Start(); // Begin execution of the background task.
        }

        /// <summary>
        /// Background task that adjusts the temperature and checks pressure every 2 seconds.
        /// </summary>
        private void BackgroundTask()
        {
            // Random number generator for simulating temperature changes.
            Random rnd = new Random();

            while (true) // Infinite loop to continuously update the gas container.
            {
                Thread.Sleep(2000); // Wait for 2 seconds before each iteration.

                lock (state.AccessLock) // Lock the state to ensure thread safety.
                {
                    if (!state.IsDestroyed) // If the container is not destroyed, proceed.
                    {
                        // Generate a random temperature change between -15K and +15K.
                        double tempChange = rnd.Next(-15, 16);
                        state.Temperature += tempChange; // Update the temperature.
                        mLog.Info($"Temperature changed by {tempChange}K. New temperature: {state.Temperature}K"); // Log the change.

                        // Check if the pressure exceeds critical limits (implosion or explosion).
                        CheckPressureLimits();
                    }
                    else
                    {
                        // If the container is destroyed, log the event and reset the state.
                        mLog.Info("Container destroyed. Resetting state.");
                        state.Reset(); // Call Reset to bring the container back to the initial state.
                    }
                }
            }
        }

        /// <summary>
        /// Checks the current pressure against the implosion and explosion limits.
        /// </summary>
        private void CheckPressureLimits()
        {
            double currentPressure = state.Pressure; // Retrieve the current pressure.
            mLog.Info($"Current pressure: {currentPressure}"); // Log the current pressure.

            if (currentPressure < state.ImplosionLimit) // If pressure drops below implosion limit.
            {
                state.IsDestroyed = true; // Mark the container as destroyed.
                mLog.Warn("Pressure dropped below implosion limit. Container imploded!"); // Log the implosion event.
            }
            else if (currentPressure > state.ExplosionLimit) // If pressure exceeds explosion limit.
            {
                state.IsDestroyed = true; // Mark the container as destroyed.
                mLog.Warn("Pressure exceeded explosion limit. Container exploded!"); // Log the explosion event.
            }
        }

        /// <summary>
        /// Increases the gas mass by a specified amount (called by input components).
        /// </summary>
        /// <param name="mass">The amount of mass to add to the gas container.</param>
        public void IncreaseMass(double mass)
        {
            lock (state.AccessLock) // Lock the state to ensure thread safety.
            {
                // Only add mass if the container is not destroyed and pressure is below the limit.
                if (!state.IsDestroyed && state.Pressure < state.PressureLimit)
                {
                    state.Mass += mass; // Add the specified mass to the container.
                    mLog.Info($"Mass increased by {mass} units. New mass: {state.Mass} units."); // Log the mass increase.
                }
                else if (state.Pressure >= state.PressureLimit) // If pressure exceeds the limit, log the event.
                {
                    mLog.Info("Pressure too high to add mass."); // Log that mass cannot be added.
                }
            }
        }

        /// <summary>
        /// Decreases the gas mass by a specified amount (called by output components).
        /// </summary>
        /// <param name="mass">The amount of mass to remove from the gas container.</param>
        public void DecreaseMass(double mass)
        {
            lock (state.AccessLock) // Lock the state to ensure thread safety.
            {
                // Only remove mass if the container is not destroyed and pressure is above the upper limit.
                if (!state.IsDestroyed && state.Pressure > state.UpperPressureLimit)
                {
                    state.Mass -= mass; // Subtract the specified mass from the container.
                    mLog.Info($"Mass decreased by {mass} units. New mass: {state.Mass} units."); // Log the mass decrease.
                }
                else if (state.Pressure <= state.UpperPressureLimit) // If pressure is below the upper limit, log the event.
                {
                    mLog.Info("Pressure too low to remove mass."); // Log that mass cannot be removed.
                }
            }
        }

        /// <summary>
        /// Gets the current pressure of the gas container.
        /// </summary>
        /// <returns>The current pressure of the gas container.</returns>
        public double GetPressure()
        {
            lock (state.AccessLock) // Lock the state to ensure thread safety.
            {
                return state.Pressure; // Return the current pressure.
            }
        }

        /// <summary>
        /// Checks if the gas container has been destroyed due to implosion or explosion.
        /// </summary>
        /// <returns>True if the container is destroyed, false otherwise.</returns>
        public bool IsDestroyed()
        {
            lock (state.AccessLock) // Lock the state to ensure thread safety.
            {
                return state.IsDestroyed; // Return whether the container is destroyed.
            }
        }
    }
}
