using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace ProtocoloLazaro
{
    public sealed class WeaponRaycast : MonoBehaviour
    {
        [SerializeField] private int magazine=6,reserve=6,damage=35;
        [SerializeField] private float interval=.4f,range=25;
        private Camera view;
        private float nextShot,reloadEnd;
        public int Magazine=>magazine;
        public int Reserve=>reserve;
        public bool IsReloading
        {
            get;
            private set;
        }
        public event Action<int,int> AmmoChanged;
        public event Action<Vector3,float> Noise;
        public event Action Fired,ReloadStarted;
        public event Action<Vector3> Impact;
        void Awake()
        {
            view=GetComponentInChildren<Camera>();
        }
        void Update()
        {
            if(!GameManager.IsPlaying)return;
            if(IsReloading&&Time.time>=reloadEnd)
            {
                int n=Mathf.Min(6-magazine,reserve);
                magazine+=n;
                reserve-=n;
                IsReloading=false;
                AmmoChanged?.Invoke(magazine,reserve);
            }
            if(Keyboard.current?.rKey.wasPressedThisFrame==true)TryReload();
            if(Mouse.current?.leftButton.wasPressedThisFrame==true)TryShoot();
        }
        public bool TryReload()
        {
            if(!GameManager.IsPlaying||IsReloading||magazine==6||reserve<=0)return false;
            IsReloading=true;
            reloadEnd=Time.time+1.5f;
            ReloadStarted?.Invoke();
            return true;
        }
        public bool TryShoot()
        {
            if(!GameManager.IsPlaying||IsReloading||Time.time<nextShot)return false;
            if(magazine<=0)
            {
                TryReload();
                return false;
            }
            magazine--;
            nextShot=Time.time+interval;
            AmmoChanged?.Invoke(magazine,reserve);
            Noise?.Invoke(transform.position,10);
            Fired?.Invoke();
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var h,range,~(1<<8),QueryTriggerInteraction.Ignore))
            {
                h.collider.GetComponentInParent<ZombieAI>()?.Damage(damage);
                Impact?.Invoke(h.point);
            }
            return true;
        }
    }
}
