using Unity.Mathematics;

using UnityEngine;

[CreateAssetMenu(fileName = "HousePrefabsSettings", menuName = "DMMFS/Settings/LevelGeneration/HousePrefabsSettings")]
public class TilePrefabsSettings : ScriptableObject {
    [Header("TilePrefabsData")]
    public GameObject[] BaseCorridorFloorTiles;
    public GameObject[] VariantCorridorFloorTiles;
    public GameObject[] BaseCorridorWallTiles;
    public GameObject[] VariantCorridorWallTiles;
    public GameObject[] BaseRoomFloorTiles;
    public GameObject[] VariantRoomFloorTiles;
    public GameObject[] BaseRoomWallTiles;
    public GameObject[] VariantRoomWallTiles;

    [Header("TestRoomPrefab")]
    [Tooltip("Size of referenced GameObject, in tiles.")]
    public int2 RoomSize;
    public RoomType RoomType;
    [Tooltip("Generates blob asset of Tiles from 2D prefab.")]
    public GameObject TestRoomPrefab;
}
