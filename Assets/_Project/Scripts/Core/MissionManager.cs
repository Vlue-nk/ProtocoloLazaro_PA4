using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class MissionManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] reinforcements;
        private readonly HashSet<PowerStation> stations = new();
        private int deployed;
        private PowerStation[] available;
        void Awake() { available=FindObjectsByType<PowerStation>(FindObjectsSortMode.None); }
        public int Activated => stations.Count;
        public PowerStation Calibrating { get; private set; }
        public bool HasSample { get; private set; }
        public float Elapsed { get; private set; }
        public event Action<int,int> ProgressChanged;
        public event Action<Vector3,float> Noise;
        public event Action Changed;
        void Update() { if (GameManager.IsPlaying) Elapsed += Time.deltaTime; }
        public void Notify() => Changed?.Invoke();
        public void EmitNoise(Vector3 position) => Noise?.Invoke(position, 24);
        public bool BeginCalibration(PowerStation station)
        {
            if (!GameManager.IsPlaying || !station || !station.FuseInstalled || station.IsActive) return false;
            if (Calibrating && Calibrating != station) return false;
            Calibrating = station;
            if (reinforcements != null && deployed < reinforcements.Length)
            {
                var reserve = reinforcements[deployed++];
                if (reserve) reserve.SetActive(true);
            }
            return true;
        }
        public void Activate(PowerStation station)
        {
            if (!GameManager.IsPlaying || !station || !station.IsActive || !stations.Add(station)) return;
            if (Calibrating == station) Calibrating = null;
            ProgressChanged?.Invoke(Activated,3);
            Notify();
        }
        public bool CollectSample()
        {
            if (!GameManager.IsPlaying || Activated != 3 || HasSample) return false;
            HasSample = true;
            FindFirstObjectByType<ExtractionDoor>()?.Unlock();
            Notify();
            return true;
        }
        public string Objective
        {
            get
            {
                if (HasSample) return "MUESTRA RECUPERADA · REGRESA A LA ESCLUSA";
                if (Activated == 3) return "[E] RECOGE LA MUESTRA EN CONTENCIÓN (SECTOR C)";
                if (Calibrating)
                {
                    int panel = Calibrating.PendingAdjustment;
                    return panel > 0 ? $"SECTOR {Calibrating.Sector} · [E] AJUSTA PANEL {panel} · ESTACIONES {Activated}/3" :
                        $"CALIBRACIÓN {Calibrating.Sector} · {Mathf.FloorToInt(Calibrating.Progress * 100)}% · ESTACIONES {Activated}/3";
                }
                foreach(var station in available)
                    if(!station.IsActive && station.FuseCollected)
                        return station.FuseInstalled ? $"[Q] INICIA CALIBRACIÓN EN ESTACIÓN {station.Sector} · {Activated}/3" : $"[E] INSTALA FUSIBLE {station.Sector} EN SU ESTACIÓN · {Activated}/3";
                return $"BUSCA FUSIBLES → [E] INSTALA → [Q] CALIBRA · ESTACIONES {Activated}/3";
            }
        }
    }
}
