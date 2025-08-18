using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using System;

public partial struct PlayerSystem : ISystem
{
    EntityManager entityManager;

    Entity playerEntity;
    Entity inputEntity;

    PlayerComponent playerComponent;
    InputComponent inputComponent;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        inputEntity = SystemAPI.GetSingletonEntity<InputComponent>();

        playerComponent = entityManager.GetComponentData<PlayerComponent>(playerEntity);
        inputComponent = entityManager.GetComponentData<InputComponent>(inputEntity);

        Move(ref state);
        Shoot(ref state);
    }

    private void Move(ref SystemState state)
    {
        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);
        playerTransform.Position += new float3(inputComponent.Movement * playerComponent.MoveSpeed * SystemAPI.Time.DeltaTime, 0);

        Vector2 dir = (Vector2)inputComponent.MousePosition - (Vector2)Camera.main.WorldToScreenPoint(playerTransform.Position);
        float angle = math.degrees(math.atan2(dir.y, dir.x));
        playerTransform.Rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        entityManager.SetComponentData(playerEntity, playerTransform);
    }

    private void Shoot(ref SystemState state)
    {
        if (inputComponent.Shoot)
        {
            for(int i = 0; i < playerComponent.NumOfBulletsToSpawn; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);

                Entity bulletEntity = entityManager.Instantiate(playerComponent.BulletPrefab);

                ECB.AddComponent(bulletEntity, new BulletComponent
                {
                    Speed = 25f
                });

                ECB.AddComponent(bulletEntity, new BulletLifeTimeComponent
                {
                    RemainingLifeTime = 1.5f
                });

                LocalTransform bulletTransform = entityManager.GetComponentData<LocalTransform>(bulletEntity);
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);

                bulletTransform.Rotation = playerTransform.Rotation;

                float randomOffset = UnityEngine.Random.Range(-playerComponent.BulletSpread, playerComponent.BulletSpread);
                bulletTransform.Position = playerTransform.Position + (playerTransform.Right() * 1.65f) + (bulletTransform.Up() * randomOffset);

                ECB.SetComponent(bulletEntity, bulletTransform);
                ECB.Playback(entityManager);
                ECB.Dispose();
            }
        }
    }

}
