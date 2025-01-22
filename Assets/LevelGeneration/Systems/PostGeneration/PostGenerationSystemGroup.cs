using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(LevelGenerationSystemGroup))]
public partial class PostGenerationSystemGroup : ComponentSystemGroup {
}
