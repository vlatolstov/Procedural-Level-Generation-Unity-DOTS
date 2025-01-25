using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Editor;
using Unity.Mathematics;
using UnityEngine;

class TilePrefabsAuthoring : MonoBehaviour {

    public TilePrefabsSettings PrefabsSettings;
}

class TilePrefabsAuthoringBaker : Baker<TilePrefabsAuthoring> {
    public override void Bake(TilePrefabsAuthoring authoring) {

        var entity = GetEntity(TransformUsageFlags.None);

        var settings = authoring.PrefabsSettings;

        CreateTilePrefabsData(entity, settings);

        CreateTileCoordinatesData(entity, settings);
    }

    private void CreateTilePrefabsData(Entity entity, TilePrefabsSettings settings) {
        //var corridorTilePrefab = GetEntity(settings.CorridorTilePrefab, TransformUsageFlags.Renderable);
        //var wallTilePrefab = GetEntity(settings.WallTilePrefab, TransformUsageFlags.Renderable);
        //var spaceTilePrefab = GetEntity(settings.SpaceTilePrefab, TransformUsageFlags.Renderable);
        //var spawnPointPrefab = GetEntity(settings.SpawnPointPrefab, TransformUsageFlags.Renderable);
        //var roomTilePrefab = GetEntity(settings.RoomTilePrefab, TransformUsageFlags.Renderable);
        //var hallTilePrefab = GetEntity(settings.HallTilePrefab, TransformUsageFlags.Renderable);

        //AddComponent(entity, new TilePrefabsData {
        //    CorridorTilePrefab = corridorTilePrefab,
        //    WallTilePrefab = wallTilePrefab,
        //    SpaceTilePrefab = spaceTilePrefab,
        //    SpawnPointPrefab = spawnPointPrefab,
        //    RoomTilePrefab = roomTilePrefab,
        //    HallTilePrefab = hallTilePrefab
        //});
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
