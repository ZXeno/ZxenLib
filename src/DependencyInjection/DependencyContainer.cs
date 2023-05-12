namespace ZxenLib.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ZxenLib.Infrastructure.Exceptions;

/// <summary>
/// The Dependency Injection container for storing registered dependency types.
/// </summary>
public class DependencyContainer
{
    private readonly Dictionary<Type, ResolvedTypeWithLifeTimeOptions> container = new();

    /// <summary>
    /// Registers a new type to the <see cref="DependencyContainer"/>
    /// </summary>
    /// <typeparam name="T">The type to resolve.</typeparam>
    /// <param name="instance">The optional instance to register.</param>
    /// <param name="options">The <see cref="LifeTimeOptions"/> for the type to register.</param>
    public void Register<T>(T? instance = null, LifeTimeOptions options = LifeTimeOptions.ContainerControlled) where T : class
    {
        if (this.container.ContainsKey(typeof(T)))
        {
            throw new Exception($"Type {typeof(T).FullName} already registered.");
        }

        ResolvedTypeWithLifeTimeOptions targetType = new (typeof(T), options)
        {
            InstanceValue = instance!,
        };

        this.container.Add(typeof(T), targetType);
    }

    /// <summary>
    /// Registers a new type to the <see cref="DependencyContainer"/>
    /// </summary>
    /// <typeparam name="TTypeToResolve">The type to resolve.</typeparam>
    /// <typeparam name="TResolvedType">The resolved type to return.</typeparam>
    /// <param name="instance">The optional instance to register. This should match the type</param>
    /// <param name="options">The <see cref="LifeTimeOptions"/> for the type to register.</param>
    public void Register<TTypeToResolve, TResolvedType>(object? instance = null, LifeTimeOptions options = LifeTimeOptions.ContainerControlled)
        where TTypeToResolve : class
        where TResolvedType : class
    {
        if (this.container.ContainsKey(typeof(TTypeToResolve)))
        {
            throw new Exception($"Type {typeof(TTypeToResolve).FullName} already registered.");
        }

        ResolvedTypeWithLifeTimeOptions targetType = new ResolvedTypeWithLifeTimeOptions(typeof(TResolvedType), options)
        {
            InstanceValue = instance!,
        };

        this.container.Add(typeof(TTypeToResolve), targetType);
    }

    /// <summary>
    /// Unregisters the type provided from the DI container.
    /// If target type is of type <see cref="IDisposable"/>, the <see cref="IDisposable.Dispose"/> method is called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void UnRegister<T>() where T : class
    {
        this.UnRegister(typeof(T));
    }

    /// <summary>
    /// Unregisters the type provided from the DI container.
    /// If target type is of type <see cref="IDisposable"/>, the <see cref="IDisposable.Dispose"/> method is called.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance">Instance of the object you wish to unregister.</param>
    public void UnRegister<T>(T instance) where T : class
    {
        this.UnRegister(typeof(T));
    }

    /// <summary>
    /// Unregisters the type provided from the DI container.
    /// </summary>
    /// <param name="typeToUnregister">The type to unregister.</param>
    public void UnRegister(Type typeToUnregister)
    {
        if (!this.container.ContainsKey(typeToUnregister))
        {
            throw new Exception($"Type {typeToUnregister} is not registered in this container.");
        }

        object resolvedObject = this.container[typeToUnregister];
        this.container.Remove(typeToUnregister);

        if (resolvedObject is IDisposable disposableType)
        {
            disposableType.Dispose();
        }
    }

    /// <summary>
    /// Determines if the provided type is registered in the container.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool IsRegistered<T>()
    {
        Type t = typeof(T);

        return this.container.ContainsKey(t);
    }

    /// <summary>
    /// Resolves the requested type from the <see cref="DependencyContainer"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to resolve.</typeparam>
    /// <returns>The resolved <see cref="Type"/>.</returns>
    public T Resolve<T>()
    {
        return (T)this.Resolve(typeof(T));
    }

    /// <summary>
    /// Resolves the requested type from the <see cref="DependencyContainer"/>.
    /// </summary>
    /// <param name="typeToResolve">The <see cref="Type"/> to resolve.</param>
    /// <returns>The resolved <see cref="Type"/>.</returns>
    /// <exception cref="TypeResolverFailedException">Type not registered.</exception>
    /// <exception cref="TypeResolverFailedException">Unable to resolve a parameter for this <see cref="Type"/>.</exception>
    public object Resolve(Type typeToResolve)
    {
        if (!this.container.ContainsKey(typeToResolve))
        {
            throw new TypeNotRegisteredException($"Unable to resolve {typeToResolve.FullName}. Type is not registered.");
        }

        ResolvedTypeWithLifeTimeOptions resolvedType = this.container[typeToResolve];
        if (resolvedType.LifeTimeOption == LifeTimeOptions.ContainerControlled && resolvedType.InstanceValue != null)
        {
            return resolvedType.InstanceValue;
        }

        ConstructorInfo constructorInfo = resolvedType.ResolvedType.GetConstructors().First();

        ParameterInfo[] paramsInfo = constructorInfo.GetParameters();
        object[] resolvedParams = new object[paramsInfo.Length];

        for (int x = 0; x < paramsInfo.Length; x++)
        {
            ParameterInfo param = paramsInfo[x];
            Type t = param.ParameterType;
            try
            {
                object res = this.Resolve(t);
                resolvedParams[x] = res;
            }
            catch (Exception ex)
            {
                throw new TypeResolverFailedException($"Unable to resolve parameter type {t} for {typeToResolve}", ex);
            }
        }

        object? retObject = Activator.CreateInstance(resolvedType.ResolvedType, resolvedParams);
        if (retObject is null)
        {
            throw new TypeResolverFailedException($"Unable to create instance of type {typeToResolve}!");
        }

        if (resolvedType.LifeTimeOption == LifeTimeOptions.ContainerControlled)
        {
            resolvedType.InstanceValue = retObject;
        }

        return retObject;
    }

    ~DependencyContainer()
    {
        while (this.container.Count > 0)
        {
            this.UnRegister(this.container.Keys.First());
        }
    }
}