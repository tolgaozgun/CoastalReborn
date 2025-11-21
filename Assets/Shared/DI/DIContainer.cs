using System;
using System.Collections.Generic;
using System.Linq;

namespace Shared.DI
{
    /// <summary>
    /// Simple dependency injection container for the game.
    /// Supports transient, singleton, and scoped lifetimes.
    /// </summary>
    public class DIContainer
    {
        private readonly Dictionary<Type, ServiceRegistration> _services = new();
        private readonly Dictionary<Type, object> _singletons = new();
        private readonly Stack<Dictionary<Type, object>> _scopes = new();

        public enum Lifetime
        {
            Transient,  // New instance every time
            Singleton,  // Single instance for container lifetime
            Scoped      // Single instance per scope
        }

        private class ServiceRegistration
        {
            public Type ServiceType { get; set; }
            public Type ImplementationType { get; set; }
            public Lifetime Lifetime { get; set; }
            public object Instance { get; set; }
            public Func<DIContainer, object> Factory { get; set; }
        }

        /// <summary>
        /// Register a service with its implementation.
        /// </summary>
        public DIContainer Register<TService, TImplementation>(Lifetime lifetime = Lifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            _services[typeof(TService)] = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TImplementation),
                Lifetime = lifetime
            };
            return this;
        }

        /// <summary>
        /// Register a service with a factory function.
        /// </summary>
        public DIContainer Register<TService>(Func<DIContainer, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            _services[typeof(TService)] = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TService),
                Lifetime = lifetime,
                Factory = container => factory(container)
            };
            return this;
        }

        /// <summary>
        /// Register a singleton instance.
        /// </summary>
        public DIContainer RegisterSingleton<TService>(TService instance)
            where TService : class
        {
            _services[typeof(TService)] = new ServiceRegistration
            {
                ServiceType = typeof(TService),
                ImplementationType = typeof(TService),
                Lifetime = Lifetime.Singleton,
                Instance = instance
            };
            _singletons[typeof(TService)] = instance;
            return this;
        }

        /// <summary>
        /// Resolve a service instance.
        /// </summary>
        public T Resolve<T>()
            where T : class
        {
            return (T)Resolve(typeof(T));
        }

        /// <summary>
        /// Resolve a service instance by type.
        /// </summary>
        public object Resolve(Type serviceType)
        {
            if (!_services.TryGetValue(serviceType, out var registration))
            {
                throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered");
            }

            switch (registration.Lifetime)
            {
                case Lifetime.Singleton:
                    if (!_singletons.TryGetValue(serviceType, out var singleton))
                    {
                        singleton = CreateInstance(registration);
                        _singletons[serviceType] = singleton;
                    }
                    return singleton;

                case Lifetime.Scoped:
                    var currentScope = _scopes.Peek();
                    if (!currentScope.TryGetValue(serviceType, out var scoped))
                    {
                        scoped = CreateInstance(registration);
                        currentScope[serviceType] = scoped;
                    }
                    return scoped;

                case Lifetime.Transient:
                default:
                    return CreateInstance(registration);
            }
        }

        /// <summary>
        /// Try to resolve a service, returning null if not found.
        /// </summary>
        public T TryResolve<T>()
            where T : class
        {
            try
            {
                return Resolve<T>();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        /// Create a new scope for scoped services.
        /// </summary>
        public IDisposable CreateScope()
        {
            _scopes.Push(new Dictionary<Type, object>());
            return new ScopeDisposable(this);
        }

        /// <summary>
        /// End the current scope.
        /// </summary>
        private void EndScope()
        {
            if (_scopes.Count > 0)
            {
                var scope = _scopes.Pop();

                // Dispose any IDisposable services in the scope
                foreach (var service in scope.Values)
                {
                    if (service is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Create an instance of a service.
        /// </summary>
        private object CreateInstance(ServiceRegistration registration)
        {
            if (registration.Instance != null)
                return registration.Instance;

            if (registration.Factory != null)
                return registration.Factory(this);

            var constructors = registration.ImplementationType.GetConstructors();
            var constructor = constructors.FirstOrDefault(c => c.IsPublic) ?? constructors.FirstOrDefault();

            if (constructor == null)
                throw new InvalidOperationException($"No suitable constructor found for {registration.ImplementationType.Name}");

            var parameters = constructor.GetParameters();
            var args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                args[i] = Resolve(parameters[i].ParameterType);
            }

            return Activator.CreateInstance(registration.ImplementationType, args);
        }

        /// <summary>
        /// IDisposable implementation for scope cleanup.
        /// </summary>
        private class ScopeDisposable : IDisposable
        {
            private readonly DIContainer _container;
            private bool _disposed;

            public ScopeDisposable(DIContainer container)
            {
                _container = container;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _container.EndScope();
                    _disposed = true;
                }
            }
        }

        /// <summary>
        /// Clear all singleton instances (useful for testing).
        /// </summary>
        public void ClearSingletons()
        {
            foreach (var singleton in _singletons.Values)
            {
                if (singleton is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _singletons.Clear();
        }
    }
}