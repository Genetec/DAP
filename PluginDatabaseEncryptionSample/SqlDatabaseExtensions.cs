namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading;
    using System.Threading.Tasks;
    using Sdk.Plugin.Objects;

    internal static class SqlDatabaseExtensions
    {
        public static IObservable<DatabaseState> ObserveState(this PluginDatabase database)
        {
            return Observable.Defer(() => database.OnStateChanged().StartWith(database.State).DistinctUntilChanged());
        }

        public static IObservable<DatabaseState> OnStateChanged(this PluginDatabase database)
        {
            return Observable.FromEventPattern<EventArgs>(handler => database.DatabaseStateChanged += handler, handler => database.DatabaseStateChanged -= handler)
                .Select(pattern => database.State);
        }

        public static Task WaitForStateAsync(this PluginDatabase database, DatabaseState state, CancellationToken token = default)
        {
            return database.OnStateChanged().StartWith(database.State).Where(currentState => currentState.Equals(state)).FirstAsync().ToTask(token);
        }
    }
}