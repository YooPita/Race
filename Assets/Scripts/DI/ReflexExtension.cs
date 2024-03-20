using Reflex.Core;
using Reflex.Injectors;
using System;

public static class ReflexExtension
{
    public static ContainerBuilder AddEntryPoint<T>(this ContainerBuilder builder, Func<Container, T> factory)
        where T : class, IInitializable
    {
        T instance = default;

        builder.AddAutoSingleton(container =>
        {
            instance = factory(container);
            return instance;
        });

        builder.OnContainerBuilt += container =>
        {
            var instance = container.Resolve<T>();
            instance.Initialize();
        };

        return builder;
    }

    public static ContainerBuilder AddAutoSingleton<T>(this ContainerBuilder builder, T instance) where T : class
    {
        Lazy<T> lazyInstance = new(() =>
        {
            AttributeInjector.Inject(instance, builder.Build());
            return instance;
        });

        builder.AddSingleton(_ => lazyInstance.Value);

        return builder;
    }

    public static ContainerBuilder AddAutoSingleton<T>(this ContainerBuilder builder, Func<Container, T> factory) where T : class
    {
        builder.AddSingleton(CreateFactory(factory));
        return builder;
    }

    public static ContainerBuilder AddAutoSingleton<T>(this ContainerBuilder builder, Func<Container, T> factory, params Type[] contracts) where T : class
    {
        builder.AddSingleton(CreateFactory(factory), contracts);
        return builder;
    }

    public static ContainerBuilder AddAutoTransient<T>(this ContainerBuilder builder, Func<Container, T> factory) where T : class
    {
        builder.AddTransient(CreateFactory(factory));
        return builder;
    }

    public static ContainerBuilder AddAutoTransient<T>(this ContainerBuilder builder, Func<Container, T> factory, params Type[] contracts) where T : class
    {
        builder.AddTransient(CreateFactory(factory), contracts);
        return builder;
    }

    private static Func<Container, T> CreateFactory<T>(Func<Container, T> factory) where T : class
    {
        return container =>
        {
            T instance = factory(container);
            AttributeInjector.Inject(instance, container);
            return instance;
        };
    }
}
