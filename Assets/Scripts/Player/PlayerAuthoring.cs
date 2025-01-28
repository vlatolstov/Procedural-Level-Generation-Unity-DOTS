using Unity.Entities;
using UnityEngine;

class PlayerAuthoring : MonoBehaviour {
}

class PlayerAuthoringBaker : Baker<PlayerAuthoring> {
    public override void Bake(PlayerAuthoring authoring) {
        var playerEntity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent<PlayerTag>(playerEntity);
        AddComponent<NeedToSpawnTag>(playerEntity);
    }
}

public struct PlayerTag : IComponentData {
}