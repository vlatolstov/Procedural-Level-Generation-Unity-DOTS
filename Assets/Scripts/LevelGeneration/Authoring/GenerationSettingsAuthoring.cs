using Unity.Entities;
using UnityEngine;

class GenerationSettingsAuthoring : MonoBehaviour {
    public LevelGenerationSettings Settings;
}

class GenerationSettingsAuthoringBaker : Baker<GenerationSettingsAuthoring> {
    public override void Bake(GenerationSettingsAuthoring authoring) {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponentObject(entity, new GenerationSettingsReferenceComnponent { Settings = authoring.Settings });
    }
}

public class GenerationSettingsReferenceComnponent : IComponentData {
    public LevelGenerationSettings Settings;
}
