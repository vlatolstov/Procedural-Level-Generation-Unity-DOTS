using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(PreGenerationSystemGroup))]
[UpdateBefore(typeof(MatrixGenerationSystem))]
partial class InitiateLevelGenerationSystem : SystemBase {
    private EntityQuery _query;

    protected override void OnCreate() {

        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GenerateLevelCommand>()
            ;

        _query = GetEntityQuery(builder);
        RequireForUpdate(_query);
    }
    protected override void OnUpdate() {

        if (SystemAPI.HasSingleton<LevelGenerationData>()) {
            return;
        }

        var settings = SystemAPI.ManagedAPI
            .GetSingleton<GenerationSettingsReferenceComnponent>()
            .Settings;

        var levelGenerationSettingsEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(levelGenerationSettingsEntity, GetLevelGenerationData(settings));
        EntityManager.AddComponentData(levelGenerationSettingsEntity, GetGenerationRandomData(settings));

        foreach ((_, var player) in SystemAPI.Query<PlayerTag>().WithEntityAccess()) {
            EntityManager.SetComponentEnabled<NeedToSpawnTag>(player, true);
        }

        EntityManager.DestroyEntity(_query);
    }

    private LevelGenerationData GetLevelGenerationData(LevelGenerationSettings settings) {
        var generationDataComponent = new LevelGenerationData {
            CellSize = settings.CellSize,
            SideRoomsGap = settings.SideRoomsGap,
            CellsPerRoom = settings.CellsPerRoom,
            LevelScale = settings.LevelScale,
            VariationChance = settings.VariationChance,
            RoomsCount = settings.RoomsCount,
            InnerZoneHallsCount = settings.InnerZoneHallsCount,
            AdditionalHallEntranceProbability = settings.AdditionalHallEntranceProbability,
            CorridorWidth = new int2(settings.MinCorridorWidth, settings.MaxCorridorWidth),
            ConnectNotMSTNodesProbability = settings.ConnectNotMSTNodesProbability,
            NodesPercentToRemove = settings.NodesPercentToRemove,
            AllowedTypesMask = settings.AllowedTypesMask
        };

        return generationDataComponent;
    }
    private GenerationRandomData GetGenerationRandomData(LevelGenerationSettings settings) {
        uint seed = settings.Seed <= 0 ? (uint)UnityEngine.Random.Range(1, int.MaxValue) : settings.Seed;
        var randomInst = new Random(seed);
        var randomComponent = new GenerationRandomData { Value = randomInst };
        return randomComponent;
    }
}

public struct GenerateLevelCommand : IComponentData { }
