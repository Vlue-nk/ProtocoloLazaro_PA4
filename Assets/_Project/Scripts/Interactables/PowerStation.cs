using System;
using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class PowerStation : MonoBehaviour
    {
        [SerializeField] private Renderer display;
        [SerializeField] private Light beacon;
        [SerializeField] private string sector = "A";
        [SerializeField, Min(1)] private float calibrationDuration = 55;
        private MissionManager mission;
        private float elapsed, nextNoise;
        private int adjustments;
        public string Sector => sector;
        public bool FuseCollected { get; private set; }
        public bool FuseInstalled { get; private set; }
        public bool IsCalibrating { get; private set; }
        public bool IsActive { get; private set; }
        public float Progress => elapsed / calibrationDuration;
        public int PendingAdjustment => IsCalibrating && adjustments < 2 && elapsed >= calibrationDuration * (adjustments + 1) / 3 ? adjustments + 1 : 0;
        public event Action Activated;
        public event Action CalibrationSignal;
        public string Prompt => IsActive ? "ESTACIÓN " + sector + " EN LÍNEA" :
            !FuseCollected ? "BUSCA EL FUSIBLE " + sector : !FuseInstalled ? "[E] INSTALAR FUSIBLE " + sector :
            (!IsCalibrating && mission && mission.Calibrating) ? "OTRA ESTACIÓN ESTÁ CALIBRANDO" :
            IsCalibrating ? "CALIBRANDO " + sector + " · " + Mathf.FloorToInt(Progress * 100) + "%" : "[Q] INICIAR CALIBRACIÓN " + sector;
        void Awake() { mission = FindFirstObjectByType<MissionManager>(); }
        public void CollectFuse() { if (GameManager.IsPlaying) { FuseCollected = true; mission?.Notify(); } }
        public void InstallFuse() { if (GameManager.IsPlaying && FuseCollected && !FuseInstalled) { FuseInstalled = true; SetColor(Color.yellow); mission?.Notify(); } }
        public void Activate(MissionManager owner)
        {
            if (!GameManager.IsPlaying || IsActive || IsCalibrating || !FuseInstalled || !owner || !owner.BeginCalibration(this)) return;
            mission = owner;
            IsCalibrating = true;
            nextNoise = 0;
            SetColor(new Color(1, .55f, .08f));
            mission.Notify();
        }
        public bool Adjust(int number)
        {
            if (!GameManager.IsPlaying || PendingAdjustment != number) return false;
            adjustments++;
            CalibrationSignal?.Invoke();
            mission.Notify();
            return true;
        }
        void Update()
        {
            if (!GameManager.IsPlaying || !IsCalibrating) return;
            if (Time.time >= nextNoise) { mission.EmitNoise(transform.position); CalibrationSignal?.Invoke(); nextNoise = Time.time + 5; }
            if (PendingAdjustment != 0) return;
            float stop = adjustments < 2 ? calibrationDuration * (adjustments + 1) / 3 : calibrationDuration;
            elapsed = Mathf.Min(stop, elapsed + Time.deltaTime);
            if (elapsed < calibrationDuration) return;
            IsCalibrating = false;
            IsActive = true;
            SetColor(Color.green);
            mission.Activate(this);
            Activated?.Invoke();
        }
        void SetColor(Color color)
        {
            if (display) { display.material.color = color; display.material.SetColor("_EmissionColor", color * 2); }
            if (beacon) beacon.color = color;
        }
    }
}
