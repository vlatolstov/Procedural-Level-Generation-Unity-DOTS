using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial class PlayerMovementSystem : SystemBase {
    private const float _speed = 0.8f;
    private bool _isSetup = false;

    protected override void OnCreate() {
        RequireForUpdate<PlayerTag>();
        RequireForUpdate<MainCameraComponent>();
        RequireForUpdate<InputData>();
    }

    protected override void OnUpdate() {
        var input = SystemAPI.GetSingleton<InputData>();
        var player = SystemAPI.GetSingletonEntity<PlayerTag>();
        var playerVelocity = SystemAPI.GetComponentRW<PhysicsVelocity>(player);
        var localToWorld = SystemAPI.GetComponent<LocalToWorld>(player);
        
        if (!_isSetup) {
            var physicsBody = SystemAPI.GetComponentRW<PhysicsMass>(player);
            physicsBody.ValueRW.InverseInertia = 0f;
            _isSetup = true;
        }

        float speed = input.LShift ? _speed * 2 : _speed;
        var forward = math.normalize(localToWorld.Value.c2.xyz);
        var right = math.normalize(localToWorld.Value.c0.xyz);
        float3 localVelocity = math.clamp((right * input.Horizontal + forward * input.Vertical), new float3(-1), new float3(1)) * speed;
        playerVelocity.ValueRW.Linear = new(localVelocity.x, playerVelocity.ValueRO.Linear.y, localVelocity.z);
    }
}
