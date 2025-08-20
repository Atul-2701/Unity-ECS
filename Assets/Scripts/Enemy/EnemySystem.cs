using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public partial struct EnemySystem : ISystem
{
    private EntityManager entityManager;

    private Entity playerEntity;

    public void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;

        playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(playerEntity);

        NativeArray<Entity> allEntities = entityManager.GetAllEntities();

        foreach(Entity entity in allEntities)
        {
            if (entityManager.HasComponent<EnemyComponent>(entity))
            {
                LocalTransform enemyTransform = entityManager.GetComponentData<LocalTransform>(entity);
                EnemyComponent enemyComponent = entityManager.GetComponentData<EnemyComponent>(entity);
                float3 moveDirection = math.normalize(playerTransform.Position - enemyTransform.Position);

                enemyTransform.Position += enemyComponent.EnemySpeed * SystemAPI.Time.DeltaTime * moveDirection;

                float3 direction = math.normalize(playerTransform.Position - enemyTransform.Position);
                float angle = math.atan2(direction.x, direction.y);
                angle -= math.radians(90f);
                quaternion lookRot = quaternion.AxisAngle(new float3(0, 0, 1), angle);
                enemyTransform.Rotation = lookRot;

                entityManager.SetComponentData(entity, enemyTransform);
            }
        }
    }
}
