using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasContract
{
    /// <summary>
    /// Interface representing a gas container service that handles gas mass and pressure operations.
    /// </summary>
    public interface IGasContainerService
    {
        /// <summary>
        /// Increases the mass of gas in the container by a specified amount.
        /// </summary>
        /// <param name="mass">The amount of mass to add to the gas container.</param>
        void IncreaseMass(double mass);

        /// <summary>
        /// Decreases the mass of gas in the container by a specified amount.
        /// </summary>
        /// <param name="mass">The amount of mass to remove from the gas container.</param>
        void DecreaseMass(double mass);

        /// <summary>
        /// Retrieves the current pressure of the gas in the container.
        /// </summary>
        /// <returns>The current pressure of the gas in the container.</returns>
        double GetPressure();

        /// <summary>
        /// Checks whether the gas container has been destroyed (either by implosion or explosion).
        /// </summary>
        /// <returns>True if the container is destroyed; otherwise, false.</returns>
        bool IsDestroyed();
    }
}
