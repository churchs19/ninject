#region License
// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 
#endregion
#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject.Infrastructure;
using Ninject.Infrastructure.Introspection;
using Ninject.Infrastructure.Language;
using Ninject.Injection;
using Ninject.Parameters;
using Ninject.Planning.Directives;
using Ninject.Planning.Targets;
#endregion

namespace Ninject.Activation.Strategies
{
    /// <summary>
    /// Injects properties on an instance during activation.
    /// </summary>
    public class PropertyInjectionStrategy : ActivationStrategy
    {
#if !WINRT
        private const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.Instance;

        private BindingFlags Flags
        {
            get
            {
                #if !NO_LCG && !SILVERLIGHT
                return Settings.InjectNonPublic ? (DefaultFlags | BindingFlags.NonPublic) : DefaultFlags;
                #else
                return DefaultFlags;
                #endif
            }
        }
#endif

        /// <summary>
        /// Gets the injector factory component.
        /// </summary>
        public IInjectorFactory InjectorFactory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInjectionStrategy"/> class.
        /// </summary>
        /// <param name="injectorFactory">The injector factory component.</param>
        public PropertyInjectionStrategy(IInjectorFactory injectorFactory)
        {
            this.InjectorFactory = injectorFactory;
        }

        /// <summary>
        /// Injects values into the properties as described by <see cref="PropertyInjectionDirective"/>s
        /// contained in the plan.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="reference">A reference to the instance being activated.</param>
        public override void Activate(IContext context, InstanceReference reference)
        {
            Ensure.ArgumentNotNull(context, "context");
            Ensure.ArgumentNotNull(reference, "reference");

            var propertyValues = context.Parameters.OfType<IPropertyValue>().ToList();

            foreach (var directive in context.Plan.GetAll<PropertyInjectionDirective>())
            {
                object value = this.GetValue(context, directive.Target, propertyValues);
                directive.Injector(reference.Instance, value);
            }

            this.AssignProperyOverrides(context, reference, propertyValues);
        }



        /// <summary>
        /// Applies user supplied override values to instance properties.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="reference">A reference to the instance being activated.</param>
        /// <param name="propertyValues">The parameter override value accessors.</param>
        private void AssignProperyOverrides(IContext context, InstanceReference reference, IList<IPropertyValue> propertyValues)
        {
#if !WINRT
            var properties = reference.Instance.GetType().GetProperties(Flags);

#else
            var properties = reference.Instance.GetType().GetRuntimeProperties().FilterPublic(Settings.InjectNonPublic);
#endif
            foreach (var propertyValue in propertyValues)
            {
                string propertyName = propertyValue.Name;
                var propertyInfo = properties.FirstOrDefault(property => string.Equals(property.Name, propertyName, StringComparison.Ordinal));

                if (propertyInfo == null)
                {
                    throw new ActivationException(ExceptionFormatter.CouldNotResolvePropertyForValueInjection(context.Request, propertyName));
                }

                var target = new PropertyInjectionDirective(propertyInfo, this.InjectorFactory.Create(propertyInfo));
                object value = this.GetValue(context, target.Target, propertyValues);
                target.Injector(reference.Instance, value);
            }
        }

        /// <summary>
        /// Gets the value to inject into the specified target.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="target">The target.</param>
        /// <param name="allPropertyValues">all property values of the current request.</param>
        /// <returns>The value to inject into the specified target.</returns>
        private object GetValue(IContext context, ITarget target, IEnumerable<IPropertyValue> allPropertyValues)
        {
            Ensure.ArgumentNotNull(context, "context");
            Ensure.ArgumentNotNull(target, "target");

            var parameter = allPropertyValues.SingleOrDefault(p => p.Name == target.Name);
            return parameter != null ? parameter.GetValue(context, target) : target.ResolveWithin(context);
        }
    }
}