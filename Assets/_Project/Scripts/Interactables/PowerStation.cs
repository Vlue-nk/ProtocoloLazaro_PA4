using System;
using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class PowerStation : MonoBehaviour
    {
        [SerializeField] private Renderer display;
        [SerializeField] private Light beacon;
        public bool IsActive
        {
            get;
            private set;
        }
        public event Action Activated;
        public void Activate(MissionManager mission)
        {
            if(IsActive||!mission||!GameManager.IsPlaying)return;
            IsActive=true;
            if(display)
            {
                display.material.color=Color.green;
                display.material.SetColor("_EmissionColor",Color.green*2);
            }
            if(beacon)beacon.color=Color.green;
            mission.Activate(this);
            Activated?.Invoke();
        }
    }
}
