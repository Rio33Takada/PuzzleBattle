using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PuzzleBattle.Application;
using PuzzleBattle.Data;
using PuzzleBattle.Domain;
using PuzzleBattle.Presentation;

namespace PuzzleBattle.Architecture.Tests
{
    public sealed class ArchitectureBoundaryTests
    {
        private const string UnityEngineAssemblyPrefix = "UnityEngine";

        [TestCase(typeof(DomainAssembly))]
        [TestCase(typeof(ApplicationAssembly))]
        [TestCase(typeof(DataAssembly))]
        public void PureCSharpAssemblies_DoNotReferenceUnityEngine(Type assemblyMarker)
        {
            IEnumerable<string> references = assemblyMarker.Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name);

            Assert.That(references, Has.None.StartsWith(UnityEngineAssemblyPrefix));
        }

        [Test]
        public void DomainAssembly_HasNoProjectAssemblyDependencies()
        {
            IEnumerable<string> projectReferences = GetProjectReferences(typeof(DomainAssembly).Assembly);

            Assert.That(projectReferences, Is.Empty);
        }

        [Test]
        public void ApplicationAssembly_DependsOnlyOnDomainProjectAssembly()
        {
            IEnumerable<string> projectReferences = GetProjectReferences(typeof(ApplicationAssembly).Assembly);

            Assert.That(projectReferences, Is.EquivalentTo(new[] { "PuzzleBattle.Domain" }));
        }

        [Test]
        public void OuterAssemblies_DoNotBecomeDomainDependencies()
        {
            string[] domainReferences = typeof(DomainAssembly).Assembly
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .ToArray();

            Assert.That(domainReferences, Does.Not.Contain(typeof(DataAssembly).Assembly.GetName().Name));
            Assert.That(domainReferences, Does.Not.Contain(typeof(PresentationAssembly).Assembly.GetName().Name));
        }

        private static IEnumerable<string> GetProjectReferences(Assembly assembly)
        {
            return assembly.GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name.StartsWith("PuzzleBattle.", StringComparison.Ordinal));
        }
    }
}
