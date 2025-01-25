using System.Collections;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Scenes;

using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
public partial class PreGenerationSystemGroup : ComponentSystemGroup {

}
