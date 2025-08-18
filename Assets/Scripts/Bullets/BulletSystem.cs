using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Burst;
using Unity.Physics;

public partial struct BulletSystem : ISystem
{
   private void OnUpdate(ref SystemState state)
    {
        EntityManager entityManager = state.EntityManager;
        NativeArray<Entity> allEntities = entityManager.GetAllEntities();

        foreach (Entity entity in allEntities)
        {
            if(entityManager.HasComponent<BulletComponent>(entity) && entityManager.HasComponent<BulletLifeTimeComponent>(entity))
            {
                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(entity);
                BulletComponent bulletComponent = entityManager.GetComponentData<BulletComponent>(entity);

                bulletTransform.Position += bulletComponent.Speed * SystemAPI.Time.DeltaTime * bulletTransform.Right();
                entityManager.SetComponentData(entity, bulletTransform);

                BulletLifeTimeComponent bulletLifeTimeComponent = entityManager.GetComponentData<BulletLifeTimeComponent>(entity);
                bulletLifeTimeComponent.RemainingLifeTime -= SystemAPI.Time.DeltaTime;

                if(bulletLifeTimeComponent.RemainingLifeTime <= 0f)
                {
                    entityManager.DestroyEntity(entity);
                    continue;
                }

                entityManager.SetComponentData(entity, bulletLifeTimeComponent);
            }
        }
    }
}
