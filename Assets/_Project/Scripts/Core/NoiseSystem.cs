using UnityEngine;
using Unity.Profiling;
namespace ProtocoloLazaro
{
    public sealed class NoiseSystem : MonoBehaviour
    {
        private readonly Collider[] receivers=new Collider[16];
        private WeaponRaycast weapon;
        private PulseEmitter pulse;
        private static readonly ProfilerMarker Detection=new("Lazaro.NoiseDetection");
        void OnEnable()
        {
            weapon=FindFirstObjectByType<WeaponRaycast>();
            pulse=FindFirstObjectByType<PulseEmitter>();
            if(weapon)weapon.Noise+=Emit;
            if(pulse)pulse.Noise+=Emit;
        }
        void OnDisable()
        {
            if(weapon)weapon.Noise-=Emit;
            if(pulse)pulse.Noise-=Emit;
        }
        public void Emit(Vector3 position,float radius)
        {
            using(Detection.Auto())
            {
                int n=Physics.OverlapSphereNonAlloc(position,radius,receivers,1<<9,QueryTriggerInteraction.Ignore);
                if(n==receivers.Length)Debug.LogError("Noise receiver capacity exhausted.");
                for(int i=0;i<n;i++)receivers[i].GetComponent<ZombieAI>()?.Hear(position);
            }
        }
    }
}
