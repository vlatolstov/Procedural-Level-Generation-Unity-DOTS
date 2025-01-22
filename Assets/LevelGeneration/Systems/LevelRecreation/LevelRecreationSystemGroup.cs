using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Scenes;


[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(PostGenerationSystemGroup))]
public partial class LevelRecreationSystemGroup : ComponentSystemGroup
{
    
}