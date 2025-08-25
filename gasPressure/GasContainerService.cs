using GasContract;

namespace GasPressure
{
    /// <summary>
    /// This class implements the IGasContainerService interface and acts as a service to communicate with the GasContainerLogic class.
    /// It provides methods for increasing and decreasing the gas mass, retrieving the pressure, and checking if the gas container is destroyed.
    /// </summary>
    public class GasContainerService : IGasContainerService
    {
        /// <summary>
        /// An instance of GasContainerLogic that holds the business logic and state of the gas container.
        /// </summary>
        private readonly GasContainerLogic mLogic = new GasContainerLogic();

        /// <summary>
        /// Increases the gas mass in the container.
        /// </summary>
        /// <param name="mass">The amount of mass to add to the gas container.</param>
        public void IncreaseMass(double mass)
        {
            // Calls the IncreaseMass method in the GasContainerLogic class to update the mass.
            mLogic.IncreaseMass(mass);
        }

        /// <summary>
        /// Decreases the gas mass in the container.
        /// </summary>
        /// <param name="mass">The amount of mass to remove from the gas container.</param>
        public void DecreaseMass(double mass)
        {
            // Calls the DecreaseMass method in the GasContainerLogic class to reduce the mass.
            mLogic.DecreaseMass(mass);
        }

        /// <summary>
        /// Retrieves the current pressure inside the gas container.
        /// </summary>
        /// <returns>The current pressure of the gas container.</returns>
        public double GetPressure()
        {
            // Calls the GetPressure method in the GasContainerLogic class to get the current pressure.
            return mLogic.GetPressure();
        }

        /// <summary>
        /// Checks if the gas container has been destroyed, either by implosion or explosion.
        /// </summary>
        /// <returns>True if the container is destroyed, otherwise false.</returns>
        public bool IsDestroyed()
        {
            // Calls the IsDestroyed method in the GasContainerLogic class to check if the container is destroyed.
            return mLogic.IsDestroyed();
        }
    }
}
