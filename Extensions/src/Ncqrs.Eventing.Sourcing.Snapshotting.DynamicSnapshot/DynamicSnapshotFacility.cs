﻿using System;
using Castle.MicroKernel.Facilities;
using Ncqrs.Domain.Storage;
using Castle.MicroKernel.Registration;
using System.Reflection;

namespace Ncqrs.Eventing.Sourcing.Snapshotting.DynamicSnapshot
{
    /// <summary>
    /// Initializes all the infrastructure for DynamicSnapshot.
    /// </summary>
    public class DynamicSnapshotFacility : AbstractFacility
    {
        private string _basePath;
        private readonly Assembly _assemblyWithAggreagateRoots;

        private readonly bool _generateDynamicSnapshotAssembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSnapshotFacility"/> class.
        /// </summary>
        /// <param name="assemblyName">The assembly name with aggregate roots.</param>
        public DynamicSnapshotFacility(string assemblyName, string basePath = null)
            : this(Assembly.Load(assemblyName), basePath)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSnapshotFacility"/> class.
        /// </summary>
        /// <param name="assemblyName">The assembly name with aggregate roots.</param>
        /// <param name="generateDynamicSnapshotAssembly">if set to <c>true</c> [generates dynamic snapshot assembly]. Default:<c>true</c></param>
        public DynamicSnapshotFacility(string assemblyName, bool generateDynamicSnapshotAssembly)
            : this(Assembly.Load(assemblyName), generateDynamicSnapshotAssembly)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSnapshotFacility"/> class.
        /// </summary>
        /// <param name="assemblyWithAggregateRoots">The assembly with aggregate roots.</param>
        public DynamicSnapshotFacility(Assembly assemblyWithAggregateRoots, string basePath = null)
            : this(assemblyWithAggregateRoots, true, basePath)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSnapshotFacility"/> class.
        /// </summary>
        /// <param name="assemblyWithAggregateRoots">The assembly with aggregate roots.</param>
        /// <param name="generateDynamicSnapshotAssembly">if set to <c>true</c> [generates dynamic snapshot assembly]. Default:<c>true</c></param>
        public DynamicSnapshotFacility(Assembly assemblyWithAggregateRoots, bool generateDynamicSnapshotAssembly, string basePath = null)
        {
            _generateDynamicSnapshotAssembly = generateDynamicSnapshotAssembly;
            _assemblyWithAggreagateRoots = assemblyWithAggregateRoots;

            if (basePath != null)
            {
                var uri = new Uri(assemblyWithAggregateRoots.CodeBase);
                _basePath = System.IO.Path.GetDirectoryName(uri.LocalPath);
            }
        }

        protected override void Init()
        {
            Kernel.Register(
                Component
                    .For<IAggregateRootCreationStrategy>()
                    .ImplementedBy<DynamicSnapshotAggregateRootCreationStrategy>(),
                Component
                    .For<IAggregateSupportsSnapshotValidator>()
                    .ImplementedBy<AggregateSupportsDynamicSnapshotValidator>(),
                Component
                    .For<IAggregateSnapshotter>()
                    .ImplementedBy<AggregateDynamicSnapshotter>(),
                Component
                    .For<IDynamicSnapshotAssembly>()
                    .ImplementedBy<DynamicSnapshotAssembly>()
                    .OnCreate((kernel, instance) =>
                        {
                            if (_generateDynamicSnapshotAssembly)
                                instance.CreateAssemblyFrom(_assemblyWithAggreagateRoots);
                        }),
                Component.For<SnapshotableAggregateRootFactory>(),
                Component.For<DynamicSnapshotAssemblyBuilder>()
                    .DynamicParameters((kernel, parameters) => 
                    {
                        parameters["basePath"] = _basePath;
                    }),
                Component.For<DynamicSnapshotTypeBuilder>(),
                Component.For<SnapshotableImplementerFactory>());
        }

    }
}
