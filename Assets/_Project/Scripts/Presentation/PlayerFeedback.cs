using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class PlayerFeedback : MonoBehaviour
    {
        [SerializeField] private AudioClip shot,pulseSound,hurt,footstep;
        [SerializeField] private Transform weaponVisual;
        [SerializeField] private ParticleSystem pulseEffect;
        private AudioSource source;
        private WeaponRaycast weapon;
        private PulseEmitter pulse;
        private PlayerHealth health;
        private CharacterController motor;
        private Vector3 home;
        private float recoil,nextStep;
        void Awake()
        {
            source=gameObject.AddComponent<AudioSource>();
            source.spatialBlend=0;
            motor=GetComponent<CharacterController>();
            if(weaponVisual)home=weaponVisual.localPosition;
        }
        void OnEnable()
        {
            weapon=GetComponent<WeaponRaycast>();
            pulse=GetComponent<PulseEmitter>();
            health=GetComponent<PlayerHealth>();
            weapon.Fired+=Shot;
            pulse.Emitted+=Pulse;
            health.Hurt+=Hurt;
        }
        void OnDisable()
        {
            weapon.Fired-=Shot;
            pulse.Emitted-=Pulse;
            health.Hurt-=Hurt;
        }
        void Shot()
        {
            source.PlayOneShot(shot,.6f);
            recoil=.12f;
        }
        void Pulse()
        {
            source.PlayOneShot(pulseSound,.65f);
            if(pulseEffect)pulseEffect.Play();
        }
        void Hurt()
        {
            source.PlayOneShot(hurt,.8f);
        }
        void Update()
        {
            if(!GameManager.IsPlaying)return;
            recoil=Mathf.MoveTowards(recoil,0,Time.deltaTime*.8f);
            if(weaponVisual)
            {
                weaponVisual.localPosition=home+new Vector3(0,weapon.IsReloading?-.25f:0,-recoil);
                weaponVisual.localRotation=Quaternion.Euler(weapon.IsReloading?30:-recoil*80,180,0);
            }
            if(motor.isGrounded&&motor.velocity.sqrMagnitude>1&&Time.time>nextStep)
            {
                source.PlayOneShot(footstep,.2f);
                nextStep=Time.time+.42f;
            }
        }
    }
}
