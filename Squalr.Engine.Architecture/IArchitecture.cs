﻿namespace Squalr.Engine.Architecture
{
    using Assembler;
    using Disassembler;
    using System;

    /// <summary>
    /// An interface defining an object that can assemble and disassemble instructions.
    /// </summary>
    public interface IArchitecture
    {
        /// <summary>
        /// Gets an instruction assembler
        /// </summary>
        /// <returns>An object implementing the IAssembler interface.</returns>
        IAssembler GetAssembler();

        /// <summary>
        /// Gets an instruction disassembler
        /// </summary>
        /// <returns>An object implementing the IDisassembler interface.</returns>
        IDisassembler GetDisassembler();

        /// <summary>
        /// Gets a value indicating if the archiecture has vector instruction support.
        /// </summary>
        /// <returns>A value indicating if the archiecture has vector instruction support.</returns>
        Boolean HasVectorSupport();

        /// <summary>
        /// Gets the vector size supported by the current architecture.
        /// If vectors are not supported, returns the lowest common denominator vector size for the architecture.
        /// </summary>
        /// <returns>The vector size.</returns>
        Int32 GetVectorSize();
    }
    //// End architecture
}
//// End namespace