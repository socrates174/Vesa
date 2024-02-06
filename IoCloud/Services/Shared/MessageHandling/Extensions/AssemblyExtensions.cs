using IoCloud.Shared.MessageHandling.Abstractions;
using System.Reflection;

namespace IoCloud.Shared.MessageHandling.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> FindFilteredTypes(this Type[] types, Type filterType)
            =>
                from type in types
                where !type.IsAbstract && !type.IsGenericTypeDefinition
                let queryInterfaces =
                    from iface in type.GetInterfaces()
                    where iface.IsGenericType
                    where iface.GetGenericTypeDefinition() == filterType
                    select iface
                where queryInterfaces.Any()
                select type;

        public static IEnumerable<Type> FindUnhandledCommands(this Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();

            var commands = assemblyTypes.FindFilteredTypes(typeof(ICommand<>)).ToList();
            var commandHandlers = assemblyTypes.FindFilteredTypes(typeof(ICommandHandler<,>)).ToList();
            var commandHandlerInterfaces = commandHandlers.SelectMany(t => t.GetInterfaces()).ToList();

            try
            {
                return (from command in commands
                        let resultType = command.GetInterfaces()
                            .Single(i => typeof(ICommand<>).IsAssignableFrom(i) && i.GetGenericArguments().Any())
                            .GetGenericArguments()
                            .First()
                        let handlerType = typeof(ICommandHandler<,>).MakeGenericType(command, resultType)
                        where commandHandlerInterfaces.Any(t => t == handlerType) == false
                        select command)
                        .ToList();
            }
            catch (Exception ex)
            {
                return new List<Type>();
            }
        }

        public static IEnumerable<Type> FindUnhandledEvents(this Assembly assembly)
        {
            var assemblyTypes = assembly.GetTypes();

            var events = assemblyTypes.FindFilteredTypes(typeof(IEvent)).ToList();
            var eventHandlers = assemblyTypes.FindFilteredTypes(typeof(IEventHandler<>)).ToList();
            var eventHandlerInterfaces = eventHandlers.SelectMany(t => t.GetInterfaces()).ToList();

            try
            {
                return (from anEvent in events
                    let resultType = anEvent.GetInterfaces()
                        .Single(i => typeof(IEvent).IsAssignableFrom(i) && i.GetGenericArguments().Any())
                        .GetGenericArguments()
                        .First()
                    let handlerType = typeof(IEventHandler<>).MakeGenericType(anEvent, resultType)
                    where eventHandlerInterfaces.Any(t => t == handlerType) == false
                    select anEvent)
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<Type>();
            }
        }
    }
}

