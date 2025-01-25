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

        var prefabs = AddBuffer<TilePrefab>(entity);
        var indexes = ConvertPrefabsToEntities(settings, ref prefabs);
        AddComponent(entity, indexes);

        CreateTileCoordinatesData(entity, settings);
    }

    private TilesIndexesInBufferComponent ConvertPrefabsToEntities(TilePrefabsSettings settings, ref DynamicBuffer<TilePrefab> buffer) {

        int index = 0;
        TilesIndexesInBufferComponent result = new();

        int start = index;
        ConvertToEntities(settings.BaseCorridorFloorTiles, ref index, ref buffer);
        int end = index - 1;
        result.BaseCorridorFloorTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.VariantCorridorFloorTiles, ref index, ref buffer);
        end = index - 1;
        result.VariantCorridorFloorTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.BaseCorridorWallTiles, ref index, ref buffer);
        end = index - 1;
        result.BaseCorridorWallTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.VariantCorridorWallTiles, ref index, ref buffer);
        end = index - 1;
        result.VariantCorridorWallTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.BaseRoomFloorTiles, ref index, ref buffer);
        end = index - 1;
        result.BaseRoomFloorTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.VariantRoomFloorTiles, ref index, ref buffer);
        end = index - 1;
        result.VariantRoomFloorTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.BaseRoomWallTiles, ref index, ref buffer);
        end = index - 1;
        result.BaseRoomWallTiles = new(start, end);
        start = index;
        ConvertToEntities(settings.VariantRoomWallTiles, ref index, ref buffer);
        end = index - 1;
        result.VariantRoomWallTiles = new(start, end);

        return result;
    }

    private void ConvertToEntities(GameObject[] source, ref int index, ref DynamicBuffer<TilePrefab> buffer) {
        foreach (var go in source) {
            TilePrefab prefab = new() {
                Prefab = GetEntity(go, TransformUsageFlags.Renderable)
            };

            buffer.Add(prefab);
            index++;
        }
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
