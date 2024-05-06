namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using Sdk;
    using Sdk.Entities;

    public static class EngineExtensions
    {
        public static IObservable<EntityInvalidatedEventArgs> OnEntityInvalidatedEvent(this Engine engine)
        {
            return Observable.FromEventPattern<EntityInvalidatedEventArgs>(
                handler => engine.EntityInvalidated += handler,
                handler => engine.EntityInvalidated -= handler).Select(pattern => pattern.EventArgs);
        }

        public static IObservable<Guid> ObserveRoleCurrentServer(this Engine engine, Role role)
        {
            return engine.ObserveProperty(role, r => r.CurrentServer);
        }

        public static IObservable<TValue> ObserveProperty<T, TValue>(this Engine engine, T entity, Func<T, TValue> func, IEqualityComparer<TValue>? comparer = null)
            where T : Entity
        {
            comparer ??= EqualityComparer<TValue>.Default;

            return Observable.Defer(Factory);

            IObservable<TValue> Factory() => engine.OnEntityInvalidatedEvent().Where(args => args.EntityGuid == entity.Guid).Select(args => func(entity)).StartWith(func(entity)).DistinctUntilChanged(comparer);
        }

        public static IObservable<LoggedOffEventArgs> OnLoggedOff(this IEngine engine)
        {
            return Observable.FromEventPattern<LoggedOffEventArgs>(
                handler => engine.LoggedOff += handler,
                handler => engine.LoggedOff -= handler).Select(pattern => pattern.EventArgs);
        }

        public static IObservable<LoggedOnEventArgs> OnLoggedOn(this IEngine engine)
        {
            return Observable.FromEventPattern<LoggedOnEventArgs>(
                handler => engine.LoggedOn += handler,
                handler => engine.LoggedOn -= handler).Select(pattern => pattern.EventArgs);
        }

        public static IObservable<bool> ObserveConnected(this IEngine engine)
        {
            return Observable.Defer(Factory);

            IObservable<bool> Factory()
            {
                var loggedOn = engine.OnLoggedOn().Select(pattern => engine.IsConnected);
                var loggedOff = engine.OnLoggedOff().Select(pattern => engine.IsConnected);

                return loggedOn.Merge(loggedOff).StartWith(engine.IsConnected).DistinctUntilChanged();
            }
        }
    }
}