using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IoCloud.Shared.MessageHandling.Abstractions;
using IoCloud.Shared.MessageHandling.Infrastructure;
using System.Reflection;

namespace IoCloud.Shared.MessageHandling.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register the IoCloud Mediator implementation.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        /// <param name="assemblies">Assemblies to scan for handlers.</param>
        /// <returns>Updated Service Collection</returns>
        /// <exception cref="ArgumentException">Argument assemblies is empty or invalid.</exception>
        public static IServiceCollection AddIocMediator(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!assemblies.Any())
            {
                throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
            }

            var assembliesToScan = assemblies.Distinct().ToArray();

            services.AddMediatR(assembliesToScan);

            // Note: Not sure if this is needed
            //var serviceConfig = new MediatRServiceConfiguration();
            //AddIocMediatorClasses(services, assembliesToScan, serviceConfig);

            services.AddSingleton<IIocMediatorFactory, IocMediatorFactory>();
            services.AddScoped<IIocMediator, IocMediator>();

            // Register Pipeline Behaviors in order of execution.
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));

            services.AddValidatorsFromAssemblies(assembliesToScan);

            return services;
        }

        public static IServiceCollection AddIocMediatorEx(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (!assemblies.Any())
            {
                throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
            }

            var assembliesToScan = assemblies.Distinct().ToArray();

            var serviceConfig = new MediatRServiceConfiguration();
            AddIocMediatorClasses(services, assembliesToScan, serviceConfig);

            return services;
        }

        private static void AddIocMediatorClasses(IServiceCollection services, IEnumerable<Assembly> assembliesToScan, MediatRServiceConfiguration configuration)
        {
            assembliesToScan = assembliesToScan.Distinct().ToArray();

            ConnectImplementationsToTypesClosing(typeof(IEventHandler<>), services, assembliesToScan, false, configuration);
            ConnectImplementationsToTypesClosing(typeof(ICommandHandler<,>), services, assembliesToScan, false, configuration);
            ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>), services, assembliesToScan, false, configuration);

            var multiOpenInterfaces = new[]
            {
                typeof(IEventHandler<>)
            };

            foreach (var multiOpenInterface in multiOpenInterfaces)
            {
                var arity = multiOpenInterface.GetGenericArguments().Length;

                var concretions = assembliesToScan
                    .SelectMany(a => a.DefinedTypes)
                    .Where(type => type.FindInterfacesThatClose(multiOpenInterface).Any())
                    .Where(type => type.IsConcrete() && type.IsOpenGeneric())
                    .Where(type => type.GetGenericArguments().Length == arity)
                    .Where(configuration.TypeEvaluator)
                    .ToList();

                foreach (var type in concretions)
                {
                    services.AddTransient(multiOpenInterface, type);
                }
            }
        }

        private static void ConnectImplementationsToTypesClosing
        (
            Type openRequestInterface,
            IServiceCollection services,
            IEnumerable<Assembly> assembliesToScan,
            bool addIfAlreadyExists,
            MediatRServiceConfiguration configuration
        )
        {
            var concretions = new List<Type>();
            var interfaces = new List<Type>();
            foreach (var type in assembliesToScan.SelectMany(a => a.DefinedTypes).Where(t => !t.IsOpenGeneric()).Where(configuration.TypeEvaluator))
            {
                var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();
                if (!interfaceTypes.Any()) continue;

                if (type.IsConcrete())
                {
                    concretions.Add(type);
                }

                foreach (var interfaceType in interfaceTypes)
                {
                    interfaces.Fill(interfaceType);
                }
            }

            foreach (var @interface in interfaces)
            {
                var exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
                if (addIfAlreadyExists)
                {
                    foreach (var type in exactMatches)
                    {
                        services.AddTransient(@interface, type);
                    }
                }
                else
                {
                    if (exactMatches.Count > 1)
                    {
                        exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
                    }

                    foreach (var type in exactMatches)
                    {
                        services.TryAddTransient(@interface, type);
                    }
                }

                if (!@interface.IsOpenGeneric())
                {
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                }
            }
        }

        private static bool IsMatchingWithInterface(Type handlerType, Type handlerInterface)
        {
            if (handlerType == null || handlerInterface == null)
            {
                return false;
            }

            if (handlerType.IsInterface)
            {
                if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                {
                    return true;
                }
            }
            else
            {
                return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return false;
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
        {
            foreach (var type in concretions
                         .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
            {
                try
                {
                    services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                }
                catch (Exception)
                {
                }
            }
        }

        private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null) return false;

            if (pluggedType == pluginType) return true;

            return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        private static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
        }

        private static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
        {
            if (pluggedType == null) yield break;

            if (!pluggedType.IsConcrete()) yield break;

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                    pluggedType.GetInterfaces()
                        .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     (pluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType))
            {
                yield return pluggedType.GetTypeInfo().BaseType;
            }

            if (pluggedType.GetTypeInfo().BaseType == typeof(object)) yield break;

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }

        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value)) return;
            list.Add(value);
        }

        /// <summary>
        /// Adds a new transient registration to the service collection only when no existing registration of the same service type and implementation type exists.
        /// In contrast to TryAddTransient, which only checks the service type.
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="serviceType">Service type</param>
        /// <param name="implementationType">Implementation type</param>
        private static void TryAddTransientExact(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            if (services.Any(reg => reg.ServiceType == serviceType && reg.ImplementationType == implementationType))
            {
                return;
            }

            services.AddTransient(serviceType, implementationType);
        }
    }
}
