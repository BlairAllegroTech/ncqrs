﻿using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Ncqrs.Eventing.Sourcing.Snapshotting.DynamicSnapshot
{
    /// <summary>
    /// Class for building snapshot assembly.
    /// </summary>
    internal sealed class DynamicSnapshotAssemblyBuilder
    {
        public const string DefaultModuleName = "DynamicSnapshot.dll";

        private readonly AssemblyBuilder _assemblyBuilder;

        private readonly string _assemblyFileName;

        private readonly ModuleBuilder _moduleBuilder;

        private readonly DynamicSnapshotTypeBuilder _typeBuilder;

        private readonly Dictionary<Type, Type> _typeRegistry = new Dictionary<Type, Type>();

        private readonly Guid AssemblyModuleVersionGuid = Guid.Parse("938bab08-4f95-430f-b1b7-2200ae4085d5");

        private readonly string _basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicSnapshotAssemblyBuilder"/> class.
        /// </summary>
        /// <param name="typeBuilder">The type builder.</param>
        public DynamicSnapshotAssemblyBuilder(DynamicSnapshotTypeBuilder typeBuilder, string basePath)
        {
            _typeBuilder = typeBuilder;
            _basePath = basePath;

            var assemblyName = new AssemblyName(DefaultModuleName);
            _assemblyFileName = DefaultModuleName;
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, _basePath);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(DefaultModuleName, _assemblyFileName );
        }

        /// <summary>
        /// Registers a snapshot type for given aggregate.
        /// </summary>
        /// <param name="aggregateType">Type of the aggregate.</param>
        /// <returns></returns>
        public Type RegisterSnapshotType(Type aggregateType)
        {
            if (aggregateType == null)
                throw new ArgumentNullException("sourceType");

            var snapshotType = GetSnapshotType(aggregateType);
            if (snapshotType != null)
                return snapshotType;

            snapshotType = _typeBuilder.CreateType(aggregateType, _moduleBuilder);

            if (snapshotType != null)
            {
                _typeRegistry.Add(aggregateType, snapshotType);
            }

            return snapshotType;
        }

        /// <summary>
        /// Saves the assembly to the file system.
        /// </summary>
        /// <returns></returns>
        public Assembly SaveAssembly()
        {
            var filePath = Path.Combine(_basePath, DefaultModuleName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            var file = DefaultModuleName;
            _assemblyBuilder.Save(file);
            //AssemblyModuleVersionId.Change(file, DefaultModuleName, AssemblyModuleVersionGuid);
            AssemblyModuleVersionId.Change(filePath, DefaultModuleName, AssemblyModuleVersionGuid);
            //return Assembly.LoadFrom(DefaultModuleName);
            return Assembly.LoadFrom(filePath);


            //AppDomain.CurrentDomain.DefineDynamicAssembly(
            //    new AssemblyName(DefaultModuleName),
            //    AssemblyBuilderAccess.Save,
            //    basePath);

            //_assemblyBuilder.DefineDynamicModule(DefaultModuleName, file);

            //AssemblyModuleVersionId.Change(file, DefaultModuleName, AssemblyModuleVersionGuid);

            //return Assembly.LoadFrom(file);
        }

        private Type GetSnapshotType(Type sourceType)
        {
            Type result;
            _typeRegistry.TryGetValue(sourceType, out result);
            return result;
        }

    }

}
