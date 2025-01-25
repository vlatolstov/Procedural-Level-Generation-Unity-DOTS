using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Editor;
using Unity.Mathematics;

using UnityEngine;

using static UnityEngine.EventSystems.EventTrigger;

class TilePrefabsAuthoring : MonoBehaviour {

    public TilePrefabsSettings PrefabsSettings;
}

class TilePrefabsAuthoringBaker : Baker<TilePrefabsAuthoring> {
    public override void Bake(TilePrefabsAuthoring authoring) {

        var entity = GetEntity(TransformUsageFlags.None);

        var settings = authoring.PrefabsSettings;

        var prefabsData = GetTilePrefabsData(settings);
        AddComponent(entity, prefabsData);

        CreateTileCoordinatesData(entity, settings);
    }

    private TilePrefabsData GetTilePrefabsData(TilePrefabsSettings settings) {
        return new() {
            BaseCorridorFloorTiles = GetEntitiesNativeArray(settings.BaseCorridorFloorTiles),
            VariantCorridorFloorTiles = GetEntitiesNativeArray(settings.VariantCorridorFloorTiles),
            BaseCorridorWallTiles = GetEntitiesNativeArray(settings.BaseCorridorWallTiles),
            VariantCorridorWallTiles = GetEntitiesNativeArray(settings.VariantCorridorWallTiles),
            BaseRoomFloorTiles = GetEntitiesNativeArray(settings.BaseRoomFloorTiles),
            VariantRoomFloorTiles = GetEntitiesNativeArray(settings.VariantRoomFloorTiles),
            BaseRoomWallTiles = GetEntitiesNativeArray(settings.BaseRoomWallTiles),
            VariantRoomWallTiles = GetEntitiesNativeArray(settings.VariantRoomWallTiles)
        };
    }

    private NativeArray<Entity> GetEntitiesNativeArray(GameObject[] source) {
        int n = source.Length;
        Entity[] target = new Entity[n];
        for (int i = 0; i < n; i++) {
            target[i] = GetEntity(source[i], TransformUsageFlags.Renderable);
        }
        NativeArray<Entity> prefabs = new(target, Allocator.Persistent);
        return prefabs;
    }

    private void CreateTileCoordinatesData(Entity entity, TilePrefabsSettings settings) {
        var roomPrefab = settings.TestRoomPrefab;
        var childrenTileInfo = roomPrefab.GetComponentsInChildren<TileAuthoring>();
        var tileArray = new NativeArray<Tile>(childrenTileInfo.Length, Allocator.Temp);

        for (int i = 0; i < childrenTileInfo.Length; i++) {
            var tileInfo = childrenTileInfo[i];
            var position = new int2((int)tileInfo.transform.position.x, (int)tileInfo.transform.position.y);
            var tile = new Tile(position, tileInfo.Element, settings.RoomType);
            tileArray[i] = tile;
        }

        var builder = new BlobBuilder(Allocator.Temp);
        ref var blob = ref builder.ConstructRoot<TestRoomTileBlob>();
        var tiles = builder.Allocate(ref blob.TileBlob, tileArray.Length);

        for (int i = 0; i < tileArray.Length; i++) {
            tiles[i] = tileArray[i];
        }

        var blobAsset = builder.CreateBlobAssetReference<TestRoomTileBlob>(Allocator.Persistent);
        AddBlobAsset(ref blobAsset, out _);
        AddComponent(entity, new TestRoomTileCoordinatesData {
            BlobReference = blobAsset,
            Size = settings.RoomSize
        });

        builder.Dispose();
    }
}

public struct TestRoomTileCoordinatesData : IComponentData {
    public BlobAssetReference<TestRoomTileBlob> BlobReference;
    public int2 Size;
}

public struct TestRoomTileBlob {
    public BlobArray<Tile> TileBlob;
}
