using System;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Registration;
using Unity.Resolution;

namespace EFBot.Shared.Scaffolding
{
    public static class UnityContainerExtensions
    {
        public static IUnityContainer RegisterSingleton<TFrom, TTo>(this IUnityContainer instance, params InjectionMember[] members)
            where TTo : TFrom
        {
            return instance.RegisterSingleton<TFrom, TTo>(null, members);
        }

        public static IUnityContainer RegisterSingleton<TFrom>(this IUnityContainer instance, params InjectionMember[] members)
        {
            return instance.RegisterSingleton(typeof(TFrom), members);
        }

        public static IUnityContainer RegisterSingleton(this IUnityContainer instance, Type from, Type to, params InjectionMember[] members)
        {
            return instance.RegisterType(from, to, new ContainerControlledLifetimeManager(), members);
        }

        public static IUnityContainer RegisterSingleton(this IUnityContainer instance, Type from, params InjectionMember[] members)
        {
            return instance.RegisterType(from, new ContainerControlledLifetimeManager(), members);
        }

        public static IUnityContainer RegisterSingleton<TFrom, TTo>(this IUnityContainer instance, string name, params InjectionMember[] members)
            where TTo : TFrom
        {
            return instance.RegisterType<TFrom, TTo>(name, new ContainerControlledLifetimeManager(), members);
        }
    }
}
