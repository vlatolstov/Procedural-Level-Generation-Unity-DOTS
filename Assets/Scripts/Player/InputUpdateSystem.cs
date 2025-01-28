using Unity.Entities;

using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
partial class InputUpdateSystem : SystemBase {
    
    protected override void OnCreate() {
        EntityManager.AddComponent<InputData>(EntityManager.CreateEntity());
    }
    protected override void OnUpdate() {
        var inputData = SystemAPI.GetSingletonRW<InputData>();
        inputData.ValueRW.Horizontal = Input.GetAxis("Horizontal");
        inputData.ValueRW.Vertical = Input.GetAxis("Vertical");
        inputData.ValueRW.MouseX = Input.GetAxis("Mouse X");
        inputData.ValueRW.MouseY = Input.GetAxis("Mouse Y");

        inputData.ValueRW.Spacebar = Input.GetKey(KeyCode.Space);
        inputData.ValueRW.LShift = Input.GetKey(KeyCode.LeftShift);
    }
}
