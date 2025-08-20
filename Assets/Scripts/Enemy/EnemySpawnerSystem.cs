using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public partial struct EnemySpawnerSystem : ISystem
{
    private EntityManager entityManager;
    private Entity enemySpawnerEntity;
    private EnemySpawnerComponent enemySpawnerComponent;
    private Entity playerEntity;

    private Unity.Mathematics.Random random;

    public void OnCreate(ref SystemState state)
    {
        random = Unity.Mathematics.Random.CreateFromIndex((uint)enemySpawnerComponent.GetHashCode());
    }

    public void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;

        enemySpawnerEntity = SystemAPI.GetSingletonEntity<EnemySpawnerComponent>();
        enemySpawnerComponent = entityManager.GetComponentData<EnemySpawnerComponent>(enemySpawnerEntity);

        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        SpawnEnemies(ref state);
    }

    private void SpawnEnemies(ref SystemState state)
    {
        enemySpawnerComponent.CurrentTimeBeforeNextSpawn -= SystemAPI.Time.DeltaTime;
        if(enemySpawnerComponent.CurrentTimeBeforeNextSpawn <= 0f)
        {
            for(int i = 0; i < enemySpawnerComponent.NumOfEnemiesToSpawnPerSecond; i++)
            {
                EntityCommandBuffer ECB = new EntityCommandBuffer(Allocator.Temp);
                Entity enemyEntity = entityManager.Instantiate(enemySpawnerComponent.EnemyPrefabToSpawn);

                LocalTransform enemyTransform = entityManager.GetComponentData<LocalTransform>(enemyEntity);
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);

                float minDistanceSquared = enemySpawnerComponent.MinimumDistanceFromPlayer * enemySpawnerComponent.MinimumDistanceFromPlayer;
                float2 randomOffset = random.NextFloat2Direction() * random.NextFloat(enemySpawnerComponent.MinimumDistanceFromPlayer, enemySpawnerComponent.EnemySpawnRadius);
                float2 playerPosition = new float2(playerTransform.Position.x, playerTransform.Position.y);
                float2 spawnPosition = playerPosition + randomOffset;
                float distanceSquared = math.lengthsq(spawnPosition - playerPosition);

                if(distanceSquared < minDistanceSquared)
                {
                    spawnPosition = playerPosition + math.normalize(randomOffset) * math.sqrt(minDistanceSquared);
                }
                enemyTransform.Position = new float3(spawnPosition.x, spawnPosition.y, 0f);

                float3 direction = math.normalize(playerTransform.Position - enemyTransform.Position);
                float angle = math.atan2(direction.x, direction.y);
                angle -= math.radians(90f);
                quaternion lookRot = quaternion.AxisAngle(new float3(0, 0, 1), angle);
                enemyTransform.Rotation = lookRot;

                ECB.SetComponent(enemyEntity, enemyTransform);

                ECB.AddComponent(enemyEntity, new EnemyComponent
                {
                    CurrentHealth = 100f,
                    EnemySpeed = 1.25f
                });

                ECB.Playback(entityManager);
                ECB.Dispose();
            }

            int desiredEnemiesPerWave = enemySpawnerComponent.NumOfEnemiesToSpawnPerSecond + enemySpawnerComponent.NumOfEnemiesToSpawnIncrementAmount;
            int enemiesPerWave = math.min(desiredEnemiesPerWave, enemySpawnerComponent.MaxNumberOfEnemiesToSpawnperSecond);
            enemySpawnerComponent.NumOfEnemiesToSpawnPerSecond = enemiesPerWave;



            enemySpawnerComponent.CurrentTimeBeforeNextSpawn = enemySpawnerComponent.TimeBeforeNextSpawn;
        }

        entityManager.SetComponentData(enemySpawnerEntity, enemySpawnerComponent);
    }
}
