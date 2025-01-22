using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(fileName = "HousePrefabsSettings", menuName = "DMMFS/Settings/LevelGeneration/HousePrefabsSettings")]
public class TilePrefabsSettings : ScriptableObject {
    [Header("TilePrefabsData")]
    public GameObject CorridorTilePrefab;
    public GameObject WallTilePrefab;
    public GameObject SpaceTilePrefab;
    public GameObject SpawnPointPrefab;
    public GameObject RoomTilePrefab;
    public GameObject HallTilePrefab;

    [Header("TestRoomPrefab")]
    public int2 RoomSize;
    public RoomType RoomType;
    public GameObject TestRoomPrefab;
}
