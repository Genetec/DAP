namespace Genetec.Dap.CodeSamples
{
    using System;
    using System.Reactive.Linq;
    using Sdk;
    using Sdk.WeakEventManagers;

    public static class NotifyEntityInvalidatedExtensions
    {
        public static IObservable<EntityInvalidatedEventArgs> OnEntityInvalidated(this INotifyEntityInvalidated engine)
        {
            return Observable.FromEventPattern<EntityInvalidatedEventArgs>(handler => engine.EntityInvalidated += handler, handler => engine.EntityInvalidated -= handler)
                .Select(pattern => pattern.EventArgs);
        }

        public static IObservable<EntitiesInvalidatedEventArgs> OnEntitiesInvalidated(this INotifyEntityInvalidated engine)
        {
            return Observable.FromEventPattern<EntitiesInvalidatedEventArgs>(handler => engine.EntitiesInvalidated += handler, handler => engine.EntitiesInvalidated -= handler)
                .Select(pattern => pattern.EventArgs);
        }
    }
}