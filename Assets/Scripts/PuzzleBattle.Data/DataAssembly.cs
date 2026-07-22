using System;
using PuzzleBattle.Application;

namespace PuzzleBattle.Data
{
    /// <summary>
    /// Identifies data-supply adapters, which depend on application ports.
    /// </summary>
    public static class DataAssembly
    {
        public static Type ApplicationBoundaryMarker
        {
            get { return typeof(IUseCase<,>); }
        }
    }
}
