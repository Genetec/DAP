// Copyright 2024 Genetec Inc.
// Licensed under the Apache License, Version 2.0. See the LICENSE file.

namespace Genetec.Dap.CodeSamples;

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Sdk;
using Sdk.Entities;

/// <summary>
/// Provides extension methods for the Entity class and its derivatives to enhance their functionality with reactive programming patterns.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    /// Creates an observable sequence from the FieldsChanged event of an entity.
    /// </summary>
    /// <param name="entity">The entity instance.</param>
    /// <returns>An IObservable<FieldsChangedEventArgs> that emits event arguments when the entity's fields change.</returns>
    public static IObservable<FieldsChangedEventArgs> OnFieldsChanged(this Entity entity)
    {
        return Observable.FromEventPattern<FieldsChangedEventArgs>(
            handler => entity.FieldsChanged += handler,
            handler => entity.FieldsChanged -= handler).Select(pattern => pattern.EventArgs);
    }

    /// <summary>
    /// Creates an observable sequence that emits the value of a property of an entity whenever the entity's fields change.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="entity">The entity instance.</param>
    /// <param name="func">A function that extracts the property value from the entity.</param>
    /// <param name="comparer">An optional equality comparer for the property values. If null, the default comparer is used.</param>
    /// <returns>An IObservable<TValue> that emits the property value when the entity's fields change or when the value changes.</returns>
    public static IObservable<TValue> ObserveProperty<T, TValue>(this T entity, Func<T, TValue> func, IEqualityComparer<TValue> comparer = null)
        where T : Entity
    {
        comparer ??= EqualityComparer<TValue>.Default;
        return Observable.Defer(() =>
            entity.OnFieldsChanged()
                .Select(_ => func(entity))
                .StartWith(func(entity))
                .DistinctUntilChanged(comparer));
    }

    /// <summary>
    /// Creates an observable sequence that emits the SpecificConfiguration of a role whenever it changes.
    /// </summary>
    /// <param name="role">The role instance.</param>
    /// <returns>An IObservable<string> that emits the SpecificConfiguration when it changes.</returns>
    public static IObservable<string> ObserveSpecificConfiguration(this Role role)
    {
        return role.ObserveProperty(entity => entity.SpecificConfiguration);
    }
}