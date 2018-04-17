﻿namespace Squalr.Source.Results
{
    using Squalr.Engine.DataTypes;

    /// <summary>
    /// Interface for a class which listens for changes in the active data type.
    /// </summary>
    internal interface IResultDataTypeObserver
    {
        /// <summary>
        /// Recieves an update of the active type in the results.
        /// </summary>
        /// <param name="activeType">The active data type.</param>
        void Update(DataType activeType);
    }
    //// End interface
}
//// End namespace