using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Platform.Interfaces;
using Platform.Exceptions;
using Platform.Disposables;
using Platform.Collections.Lists;
using Platform.Reflection;
using Platform.Singletons;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Scopes
{
    public class Scope : DisposableBase
    {
        public static readonly Scope Global = new Scope(autoInclude: true, autoExplore: true);

        private readonly bool _autoInclude;
        private readonly bool _autoExplore;
        private readonly Stack<object> _dependencies = new Stack<object>();
        private readonly HashSet<object> _excludes = new HashSet<object>();
        private readonly HashSet<object> _includes = new HashSet<object>();
        private readonly HashSet<object> _blocked = new HashSet<object>();
        private readonly Dictionary<Type, object> _resolutions = new Dictionary<Type, object>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(bool autoInclude, bool autoExplore)
        {
            _autoInclude = autoInclude;
            _autoExplore = autoExplore;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope(bool autoInclude) : this(autoInclude, false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Scope() { }

        #region Exclude

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExcludeAssemblyOf<T>() => ExcludeAssemblyOfType(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExcludeAssemblyOfType(Type type) => ExcludeAssembly(type.GetAssembly());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ExcludeAssembly(Assembly assembly) => assembly.GetCachedLoadableTypes().ForEach(Exclude);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude<T>() => Exclude(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Exclude(object @object) => _excludes.Add(@object);

        #endregion

        #region Include

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncludeAssemblyOf<T>() => IncludeAssemblyOfType(typeof(T));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncludeAssemblyOfType(Type type) => IncludeAssembly(type.GetAssembly());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void IncludeAssembly(Assembly assembly) => assembly.GetExportedTypes().ForEach(Include);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include<T>()
        {
            var types = Types<T>.Array;
            if (types.Length > 0)
            {
                types.ForEach(Include);
            }
            else
            {
                Include(typeof(T));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Include(object @object)
        {
            if (@object == null)
            {
                return;
            }
            if (_includes.Add(@object))
            {
                var type = @object as Type;
                if (type != null)
                {
                    type.GetInterfaces().ForEach(Include);
                    Include(type.GetBaseType());
                }
            }
        }

        #endregion

        #region Use

        /// <remarks>
        /// TODO: Use Default[T].Instance if the only constructor object has is parameterless.
        /// TODO: Think of interface chaining IDoubletLinks[T] (default) -> IDoubletLinks[T] (checker) -> IDoubletLinks[T] (synchronizer) (may be UseChain[IDoubletLinks[T], Types[DefaultLinks, DefaultLinksDependencyChecker, DefaultSynchronizedLinks]]
        /// TODO: Add support for factories
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Use<T>()
        {
            if (_excludes.Contains(typeof(T)))
            {
                throw new InvalidOperationException($"Type {typeof(T).Name} is excluded and cannot be used.");
            }
            if (_autoInclude)
            {
                Include<T>();
            }
            if (!TryResolve(out T resolved))
            {
                throw new InvalidOperationException($"Dependency of type {typeof(T).Name} cannot be resolved.");
            }
            if (!_autoInclude)
            {
                Include<T>();
            }
            Use(resolved);
            return resolved;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T UseSingleton<T>(IFactory<T> factory) => UseAndReturn(Singleton.Get(factory));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T UseSingleton<T>(Func<T> creator) => UseAndReturn(Singleton.Get(creator));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T UseAndReturn<T>(T @object)
        {
            Use(@object);
            return @object;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Use(object @object)
        {
            Include(@object);
            _dependencies.Push(@object);
        }

        #endregion

        #region Resolve

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResolve<T>(out T resolved)
        {
            resolved = default;
            var result = TryResolve(typeof(T), out object resolvedObject);
            if (result)
            {
                resolved = (T)resolvedObject;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryResolve(Type requiredType, out object resolved)
        {
            resolved = null;
            if (!_blocked.Add(requiredType))
            {
                return false;
            }
            try
            {
                if (_excludes.Contains(requiredType))
                {
                    return false;
                }
                if (_resolutions.TryGetValue(requiredType, out resolved))
                {
                    return true;
                }
                if (_autoExplore)
                {
                    IncludeAssemblyOfType(requiredType);
                }
                var resultInstances = new List<object>();
                var resultConstructors = new List<ConstructorInfo>();
                foreach (var include in _includes)
                {
                    if (!_excludes.Contains(include))
                    {
                        var type = include as Type;
                        if (type != null)
                        {
                            if (requiredType.IsAssignableFrom(type))
                            {
                                resultConstructors.AddRange(GetValidConstructors(type));
                            }
                            else if (type.GetTypeInfo().IsGenericTypeDefinition && requiredType.GetTypeInfo().IsGenericType && type.GetInterfaces().Any(x => x.Name == requiredType.Name))
                            {
                                var genericType = type.MakeGenericType(requiredType.GenericTypeArguments);
                                if (requiredType.IsAssignableFrom(genericType))
                                {
                                    resultConstructors.AddRange(GetValidConstructors(genericType));
                                }
                            }
                        }
                        else if (requiredType.IsInstanceOfType(include) || requiredType.IsAssignableFrom(include.GetType()))
                        {
                            resultInstances.Add(include);
                        }
                    }
                }
                if (resultInstances.Count == 0 && resultConstructors.Count == 0)
                {
                    return false;
                }
                else if (resultInstances.Count > 0)
                {
                    resolved = resultInstances[0];
                }
                else
                {
                    SortConstructors(resultConstructors);
                    if (!TryResolveInstance(resultConstructors, out resolved))
                    {
                        return false;
                    }
                }
                _resolutions.Add(requiredType, resolved);
                return true;
            }
            finally
            {
                _blocked.Remove(requiredType);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void SortConstructors(List<ConstructorInfo> resultConstructors) => resultConstructors.Sort((x, y) => -x.GetParameters().Length.CompareTo(y.GetParameters().Length));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool TryResolveInstance(List<ConstructorInfo> constructors, out object resolved)
        {
            for (var i = 0; i < constructors.Count; i++)
            {
                try
                {
                    var resultConstructor = constructors[i];
                    if (TryResolveConstructorArguments(resultConstructor, out object[] arguments))
                    {
                        resolved = resultConstructor.Invoke(arguments);
                        return true;
                    }
                }
                catch (Exception exception)
                {
                    exception.Ignore();
                }
            }
            resolved = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ConstructorInfo[] GetValidConstructors(Type type)
        {
            var constructors = type.GetConstructors();
            if (!_autoExplore)
            {
                constructors = constructors.ToArray(x =>
                {
                    var parameters = x.GetParameters();
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        if (!_includes.Contains(parameters[i].ParameterType))
                        {
                            return false;
                        }
                    }
                    return true;
                });
            }
            return constructors;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryResolveConstructorArguments(ConstructorInfo constructor, out object[] arguments)
        {
            var parameters = constructor.GetParameters();
            arguments = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!TryResolve(parameters[i].ParameterType, out object argument))
                {
                    return false;
                }
                Use(argument);
                arguments[i] = argument;
            }
            return true;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                while (_dependencies.Count > 0)
                {
                    _dependencies.Pop().DisposeIfPossible();
                }
            }
        }
    }
}
