using System;
using PuzzleBattle.Application;
using UnityEngine;

namespace PuzzleBattle.Presentation
{
    /// <summary>
    /// Unity-facing adapter boundary. Game rules must remain outside this assembly.
    /// </summary>
    public sealed class PresentationAssembly : MonoBehaviour
    {
        public static Type ApplicationBoundaryMarker
        {
            get { return typeof(IUseCase<,>); }
        }
    }
}
