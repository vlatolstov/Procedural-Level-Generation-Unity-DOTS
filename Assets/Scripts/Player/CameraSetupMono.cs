using Unity.Entities;

using UnityEngine;

public class CameraSetupMono : MonoBehaviour {
    [SerializeField] private float _sensitivity;
    void Start() {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var cameraEntity = entityManager.CreateEntity();
        entityManager.AddComponentObject(cameraEntity, new MainCameraComponent {
            Camera = gameObject,
            Sensitivity = _sensitivity
        });
    }
}
public class MainCameraComponent : IComponentData {
    public float Sensitivity;
    public GameObject Camera;
}