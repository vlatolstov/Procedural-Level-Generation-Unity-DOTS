
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using UnityEngine;

partial class CameraPositioningSystem : SystemBase {

    private float _xRotation = 0f;
    protected override void OnCreate() {
        RequireForUpdate<MainCameraComponent>();
        RequireForUpdate<PlayerTag>();
    }

    protected override void OnUpdate() {
        var input = SystemAPI.GetSingleton<InputData>();
        var playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerTransform = SystemAPI.GetComponentRW<LocalTransform>(playerEntity);
        var cameraEntity = SystemAPI.ManagedAPI.GetSingletonEntity<MainCameraComponent>();
        var cameraComponent = SystemAPI.ManagedAPI.GetSingleton<MainCameraComponent>();

        cameraComponent.Camera.transform.position = new(playerTransform.ValueRO.Position.x, playerTransform.ValueRO.Position.y + 0.1f, playerTransform.ValueRO.Position.z);


        _xRotation -= input.MouseY * cameraComponent.Sensitivity;
        _xRotation = math.clamp(_xRotation, -90f, 90f);

        float yRotation = math.degrees(math.Euler(playerTransform.ValueRO.Rotation)).y;
        yRotation += input.MouseX * cameraComponent.Sensitivity;
        cameraComponent.Camera.transform.rotation = Quaternion.Euler(_xRotation, yRotation, 0);
        playerTransform.ValueRW.Rotation = quaternion.Euler(0, math.radians(yRotation), 0);
    }
}