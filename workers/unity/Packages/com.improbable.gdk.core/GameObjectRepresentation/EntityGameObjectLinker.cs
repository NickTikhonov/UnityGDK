using System;
using System.Collections.Generic;
using Improbable.Gdk.Core;
using Improbable.Worker;
using Unity.Entities;
using UnityEngine;

namespace Improbable.Gdk.GameObjectRepresentation
{
    public class EntityGameObjectLinker
    {
        private readonly World world;
        private readonly WorkerSystem worker;
        private readonly EntityManager entityManager;
        private readonly HashSet<Type> gameObjectComponentTypes = new HashSet<Type>();

        public EntityGameObjectLinker(World world, WorkerSystem worker)
        {
            this.world = world;
            this.worker = worker;
            entityManager = world.GetExistingManager<EntityManager>();
        }

        public void LinkGameObjectToEntity(GameObject gameObject, Entity entity, EntityId spatialEntityId,
            ViewCommandBuffer viewCommandBuffer)
        {
            gameObjectComponentTypes.Clear();
            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (ReferenceEquals(component, null))
                {
                    continue;
                }

                var componentType = component.GetType();
                if (gameObjectComponentTypes.Contains(componentType))
                {
                    worker.LogDispatcher.HandleLog(LogType.Warning, new LogEvent(
                            "GameObject contains multiple instances of the same component type. Only one instance of each component type will be added to the corresponding ECS entity.")
                        .WithField("EntityId", spatialEntityId)
                        .WithField("ComponentType", componentType));
                    continue;
                }

                gameObjectComponentTypes.Add(componentType);
                viewCommandBuffer.AddComponent(entity, component.GetType(), component);
            }

            viewCommandBuffer.AddComponent(entity, new GameObjectReference { GameObject = gameObject });

            var spatialOSComponent = gameObject.AddComponent<SpatialOSComponent>();
            spatialOSComponent.World = world;
            spatialOSComponent.Worker = worker;
            spatialOSComponent.Entity = entity;
            spatialOSComponent.SpatialEntityId = spatialEntityId;
        }

        public void UnlinkGameObjectFromEntity(GameObject gameObject, Entity entity, ViewCommandBuffer viewCommandBuffer)
        {
            if (entityManager.Exists(entity))
            {
                foreach (var component in gameObject.GetComponents<Component>())
                {
                    if (ReferenceEquals(component, null))
                    {
                        continue;
                    }

                    var componentType = component.GetType();
                    if (entityManager.HasComponent(entity, componentType))
                    {
                        viewCommandBuffer.RemoveComponent(entity, componentType);
                    }
                }

                if (entityManager.HasComponent<GameObjectReference>(entity))
                {
                    viewCommandBuffer.RemoveComponent(entity, typeof(GameObjectReference));
                }
            }

            var spatialOSComponent = gameObject.GetComponent<SpatialOSComponent>();
            if (spatialOSComponent != null)
            {
                UnityObjectDestroyer.Destroy(spatialOSComponent);
            }
        }
    }
}
