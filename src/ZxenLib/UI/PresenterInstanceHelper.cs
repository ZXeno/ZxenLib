namespace ZxenLib.UI
{
    using NanoDiCs;
    using System;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Helps with the construction of PresenterModels
    /// </summary>
    public static class PresenterInstanceHelper
    {
        /// <summary>
        /// Instantiates and resolves constructor dependencies for presenter models.
        /// </summary>
        /// <typeparam name="T">The type to instantiate.</typeparam>
        /// <returns>The instantiated <see cref="Type"/></returns>
        public static T InstantiateModel<T>(DependencyContainer dependencyContainer)
            where T : PresenterModelBase
        {
            ConstructorInfo constructorInfo = typeof(T).GetConstructors().First();
            ParameterInfo[] paramsInfo = constructorInfo.GetParameters();
            object[] resolvedParams = new object[paramsInfo.Length];

            for (int x = 0; x < paramsInfo.Length; x++)
            {
                resolvedParams[x] = dependencyContainer.Resolve(paramsInfo[x].ParameterType);
            }

            return (T)Activator.CreateInstance(typeof(T), resolvedParams.ToArray());
        }
    }
}
