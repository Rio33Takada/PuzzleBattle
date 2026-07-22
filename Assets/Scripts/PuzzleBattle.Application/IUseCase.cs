using System;
using PuzzleBattle.Domain;

namespace PuzzleBattle.Application
{
    /// <summary>
    /// Input boundary implemented by application use cases.
    /// </summary>
    public interface IUseCase<in TRequest, out TResult>
    {
        TResult Execute(TRequest request);
    }

    /// <summary>
    /// Provides an assembly anchor while making the inward domain dependency explicit.
    /// </summary>
    public static class ApplicationAssembly
    {
        public static Type DomainAssemblyMarker
        {
            get { return typeof(DomainAssembly); }
        }
    }
}
