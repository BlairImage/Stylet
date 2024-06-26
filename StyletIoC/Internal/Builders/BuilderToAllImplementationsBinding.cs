﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using StyletIoC.Creation;

namespace StyletIoC.Internal.Builders;

internal class BuilderToAllImplementationsBinding : BuilderBindingBase
{
    private readonly IEnumerable<Assembly> assemblies;
    private readonly bool allowZeroImplementations;

    private BuilderTypeKey serviceType => this.ServiceTypes[0];

    public BuilderToAllImplementationsBinding(List<BuilderTypeKey> serviceTypes, IEnumerable<Assembly> assemblies, bool allowZeroImplementations)
        : base(serviceTypes)
    {
        // This should be ensured by the fluent interfaces
        Trace.Assert(this.ServiceTypes.Count == 1);

        this.assemblies = assemblies;
        this.allowZeroImplementations = allowZeroImplementations;
    }

    public override void Build(Container container)
    {
        var candidates = (from type in this.assemblies.Distinct().SelectMany(x => x.GetTypes())
                         let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.serviceType.Type || (x.IsGenericType && x.GetGenericTypeDefinition() == this.serviceType.Type))
                         where baseType != null
                         select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType }).ToList();

        if (!this.allowZeroImplementations && candidates.Count == 0)
        {
            throw new StyletIoCRegistrationException(string.Format("Did not find any implementations of the type {0}", this.serviceType.Type));
        }

        foreach (var candidate in candidates)
        {
            try
            {
                BuilderBindingBase.EnsureType(candidate.Type, candidate.Base);
                this.BindImplementationToSpecificService(container, candidate.Type, candidate.Base, this.serviceType.Key);
            }
            catch (StyletIoCRegistrationException e)
            {
                Debug.WriteLine(string.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.GetDescription(), e.Message), "StyletIoC");
            }
        }
    }
}
