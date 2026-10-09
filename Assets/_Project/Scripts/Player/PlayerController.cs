using UnityEngine;
using UnityEngine.InputSystem;
namespace ProtocoloLazaro
{
    [RequireComponent(typeof(CharacterController))] public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float walkSpeed=4.5f,runSpeed=7,sensitivity=.12f;
        private CharacterController controller;
        private Camera view;
        private float pitch,verticalSpeed;
        public string Prompt
        {
            get;
            private set;
        }
        ="";
        public float Sensitivity
        {
            get=>sensitivity;
            set=>sensitivity=Mathf.Clamp(value,.03f,.3f);
        }
        void Awake()
        {
            controller=GetComponent<CharacterController>();
            view=GetComponentInChildren<Camera>();
        }
        void Update()
        {
            if(!GameManager.IsPlaying||Keyboard.current==null||Mouse.current==null)return;
            var k=Keyboard.current;
            float x=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),z=(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0);
            Vector3 move=Vector3.ClampMagnitude(transform.right*x+transform.forward*z,1)*(k.leftShiftKey.isPressed?runSpeed:walkSpeed);
            verticalSpeed=controller.isGrounded&&verticalSpeed<0?-2:verticalSpeed-20*Time.deltaTime;
            move.y=verticalSpeed;
            controller.Move(move*Time.deltaTime);
            Vector2 look=Mouse.current.delta.ReadValue()*sensitivity;
            transform.Rotate(0,look.x,0);
            pitch=Mathf.Clamp(pitch-look.y,-80,80);
            view.transform.localRotation=Quaternion.Euler(pitch,0,0);
            Prompt="";
            if(!Physics.Raycast(view.transform.position,view.transform.forward,out var h,3,~(1<<8),QueryTriggerInteraction.Ignore))return;
            var station=h.collider.GetComponentInParent<PowerStation>();
            if(station)Prompt=station.IsActive?"ESTACIÓN EN LÍNEA":"[Q] REINICIAR ESTACIÓN";
            if(h.distance>2.5f)return;
            var door=h.collider.GetComponentInParent<ExtractionDoor>();
            var med=h.collider.GetComponentInParent<Medkit>();
            if(door)
            {
                Prompt=door.IsUnlocked?"[E] EXTRAER LA MUESTRA":"EXTRACCIÓN BLOQUEADA · ACTIVA LAS 3 ESTACIONES";
                if(k.eKey.wasPressedThisFrame)door.TryOpen(GameManager.Instance);
            }
            if(med)
            {
                Prompt="[E] BOTIQUÍN · +30 HP";
                if(k.eKey.wasPressedThisFrame)med.Use(GetComponent<PlayerHealth>());
            }
        }
    }
}
