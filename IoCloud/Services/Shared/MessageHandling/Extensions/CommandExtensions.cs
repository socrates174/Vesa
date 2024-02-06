using IoCloud.Shared.MessageHandling.Abstractions;

namespace IoCloud.Shared.MessageHandling.Extensions
{
    public static class CommandExtensions
    {
        static IList<Type> unhandledCommands = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.FindUnhandledCommands())?.ToList();
        public static bool HasHandler(this IBaseCommand command)
        {
            return !unhandledCommands.Contains(command.GetType());
        }
    }
}
