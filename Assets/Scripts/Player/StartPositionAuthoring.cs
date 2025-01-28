using Unity.Entities;

using UnityEngine;

class StartPositionAuthoring : MonoBehaviour {

}

class StartPositionAuthoringBaker : Baker<StartPositionAuthoring> {
    public override void Bake(StartPositionAuthoring authoring) {
        GetEntity(TransformUsageFlags.Renderable);
    }
}
