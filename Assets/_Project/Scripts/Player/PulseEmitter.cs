using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Profiling;
namespace ProtocoloLazaro
{
    public sealed class PulseEmitter : MonoBehaviour
    {
        [SerializeField] private float cooldown=8,radius=4.5f,stun=3;
        private Camera view;
        private MissionManager mission;
        private float ready;
        private readonly Collider[] buffer=new Collider[16];
        private static readonly ProfilerMarker Detection=new("Lazaro.PulseDetection");
        public float Remaining=>Mathf.Max(0,ready-Time.time);
        public event Action<float> CooldownChanged;
        public event Action<Vector3,float> Noise;
        public event Action Emitted;
        void Awake()
        {
            view=GetComponentInChildren<Camera>();
            mission=FindFirstObjectByType<MissionManager>();
        }
        void Update()
        {
            if(!GameManager.IsPlaying)return;
            if(Keyboard.current?.qKey.wasPressedThisFrame==true)TryPulse();
            CooldownChanged?.Invoke(Remaining/cooldown);
        }
        public bool TryPulse()
        {
            if(!GameManager.IsPlaying||Remaining>0)return false;
            ready=Time.time+cooldown;
            using(Detection.Auto())
            {
                int n=Physics.OverlapSphereNonAlloc(transform.position,radius,buffer,1<<9,QueryTriggerInteraction.Ignore);
                if(n==buffer.Length)Debug.LogError("Pulse receiver capacity exhausted.");
                for(int i=0;i<n;i++)
                {
                    var z=buffer[i].GetComponent<ZombieAI>();
                    if(z&&!Physics.Linecast(view.transform.position,z.transform.position+Vector3.up,1<<10,QueryTriggerInteraction.Ignore))z.Stun(stun);
                }
            }
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var h,3,~(1<<8),QueryTriggerInteraction.Ignore))h.collider.GetComponentInParent<PowerStation>()?.Activate(mission);
            Noise?.Invoke(transform.position,18);
            Emitted?.Invoke();
            return true;
        }
    }
}
