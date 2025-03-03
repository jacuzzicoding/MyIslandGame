using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MyIslandGame.Core.Resources
{
    /// <summary>
    /// Manages resource definitions and provides access to resources throughout the game.
    /// </summary>
    public class ResourceManager
    {
        private readonly Dictionary<string, Resource> _resources = new Dictionary<string, Resource>();
        private static ResourceManager _instance;
        private readonly GraphicsDevice _graphicsDevice;

        /// <summary>
        /// Gets the singleton instance of the ResourceManager.
        /// </summary>
        public static ResourceManager Instance => _instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceManager"/> class.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device used to create placeholder textures.</param>
        public ResourceManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
            _instance = this;
            InitializeResources();
        }

        /// <summary>
        /// Gets a resource by its unique identifier.
        /// </summary>
        /// <param name="id">The resource identifier.</param>
        /// <returns>The resource if found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the resource is not found.</exception>
        public Resource GetResource(string id)
        {
            if (_resources.TryGetValue(id, out var resource))
            {
                return resource;
            }

            throw new KeyNotFoundException($"Resource with ID '{id}' not found.");
        }

        /// <summary>
        /// Tries to get a resource by its unique identifier.
        /// </summary>
        /// <param name="id">The resource identifier.</param>
        /// <param name="resource">The found resource, or null if not found.</param>
        /// <returns>True if the resource was found, otherwise false.</returns>
        public bool TryGetResource(string id, out Resource resource)
        {
            return _resources.TryGetValue(id, out resource);
        }

        /// <summary>
        /// Gets all registered resources.
        /// </summary>
        /// <returns>An enumerable collection of resources.</returns>
        public IEnumerable<Resource> GetAllResources()
        {
            return _resources.Values;
        }

        /// <summary>
        /// Gets all resources of a specific category.
        /// </summary>
        /// <param name="category">The resource category to filter by.</param>
        /// <returns>An enumerable collection of resources in the specified category.</returns>
        public IEnumerable<Resource> GetResourcesByCategory(ResourceCategory category)
        {
            foreach (var resource in _resources.Values)
            {
                if (resource.Category == category)
                {
                    yield return resource;
                }
            }
        }

        /// <summary>
        /// Registers a new resource with the manager.
        /// </summary>
        /// <param name="resource">The resource to register.</param>
        /// <exception cref="ArgumentException">Thrown if a resource with the same ID already exists.</exception>
        public void RegisterResource(Resource resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (_resources.ContainsKey(resource.Id))
            {
                throw new ArgumentException($"A resource with ID '{resource.Id}' is already registered.");
            }

            // Create placeholder texture if none exists
            if (resource.Icon == null)
            {
                resource.Icon = resource.CreatePlaceholderTexture(_graphicsDevice);
            }

            _resources.Add(resource.Id, resource);
        }

        /// <summary>
        /// Initializes the default set of resources.
        /// </summary>
        private void InitializeResources()
        {
            // Organic resources
            RegisterResource(new Resource("wood", "Wood", "A versatile building material from trees.", ResourceCategory.Organic, 50));
            RegisterResource(new Resource("leaves", "Leaves", "Green foliage from trees and plants.", ResourceCategory.Organic, 100));
            RegisterResource(new Resource("berries", "Berries", "Small edible fruits from bushes.", ResourceCategory.Organic, 30));
            RegisterResource(new Resource("seeds", "Seeds", "Can be planted to grow new plants.", ResourceCategory.Organic, 40));
            RegisterResource(new Resource("fibers", "Plant Fibers", "Tough strands collected from certain plants.", ResourceCategory.Organic, 60));

            // Mineral resources
            RegisterResource(new Resource("stone", "Stone", "A solid building material.", ResourceCategory.Mineral, 40));
            RegisterResource(new Resource("sand", "Sand", "Fine rock particles found on beaches.", ResourceCategory.Mineral, 80));
            RegisterResource(new Resource("clay", "Clay", "Malleable earth found near water.", ResourceCategory.Mineral, 50));
            RegisterResource(new Resource("flint", "Flint", "A hard rock that can create sparks.", ResourceCategory.Mineral, 30));

            // Environmental resources
            RegisterResource(new Resource("water", "Water", "Essential liquid found in lakes and rivers.", ResourceCategory.Environmental, 20));
            RegisterResource(new Resource("soil", "Soil", "Rich earth for growing plants.", ResourceCategory.Environmental, 60));
        }
    }
}
