using Unity.Entities;

using UnityEditor;

using UnityEngine;


[CreateAssetMenu(fileName = "LevelGenerationSettings", menuName = "DMMFS/Settings/LevelGeneration/LevelGenerationSettings")]
public class LevelGenerationSettings : ScriptableObject {

    [Header("GenerationRandomData")]
    public uint Seed;

    [Space]
    [Header("LevelPropertiesData")]
    public int CellSize;
    public int SideRoomsGap;
    public int CellsPerRoom;
    [Tooltip("Scale to convert level from integer based matrix.")]
    public float LevelScale;
    [Tooltip("Probability of creating tile variant instead of base, in percents, value will be clamped in range 0 to 100.")]
    public int VariationChance;

    [Space]
    [Header("RoomsPropertiesData")]
    public int RoomsCount;
    public int InnerZoneHallsCount;
    [Tooltip("Probability of adding aditional entrances in hall, in percents, value will be clamped in range 0 to 100. Value will reduce by 25% after each iterration.")]
    public int AdditionalHallEntranceProbability;

    [Space]
    [Header("CorridorsPropertiesData")]
    public int MinCorridorWidth;
    public int MaxCorridorWidth;
    [Tooltip("Probability of adding aditional connections between corridors, in percents, value will be clamped in range 0 to 100.")]
    public int ConnectNotMSTNodesProbability;
    public int NodesPercentToRemove;

    [Space]
    [Header("SpawnPointsPropertiesData")]
    public RoomType AllowedTypesMask;

    public void GenerateLevel() {
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        em.CreateEntity(ComponentType.ReadWrite<GenerateLevelCommand>());
    }
}

[CustomEditor(typeof(LevelGenerationSettings))]
public class LevelGenerationSettingsEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        LevelGenerationSettings settings = (LevelGenerationSettings)target;
        if (GUILayout.Button("Generate level")) {
            settings.GenerateLevel();
        }
    }
}
