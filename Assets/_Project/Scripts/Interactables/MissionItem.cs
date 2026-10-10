using UnityEngine;
namespace ProtocoloLazaro
{
    public enum MissionItemKind { Fuse, AdjustmentOne, AdjustmentTwo, Sample }
    public sealed class MissionItem : MonoBehaviour
    {
        [SerializeField] private MissionItemKind kind;
        [SerializeField] private PowerStation station;
        private MissionManager mission;
        public MissionItemKind Kind => kind;
        public PowerStation Station => station;
        void Awake() { mission = FindFirstObjectByType<MissionManager>(); }
        public string Prompt => kind == MissionItemKind.Fuse ? "[E] RECOGER FUSIBLE " + station.Sector :
            kind == MissionItemKind.Sample ? (mission.Activated == 3 ? "[E] RECOGER MUESTRA" : "MUESTRA BLOQUEADA · COMPLETA LAS ESTACIONES") :
            station.PendingAdjustment == Number ? "[E] AJUSTAR PANEL " + Number + " · SECTOR " + station.Sector : "PANEL " + Number + " · EN ESPERA";
        private int Number => kind == MissionItemKind.AdjustmentOne ? 1 : 2;
        public void Use()
        {
            if (!GameManager.IsPlaying) return;
            if (kind == MissionItemKind.Fuse) { station.CollectFuse(); gameObject.SetActive(false); }
            else if (kind == MissionItemKind.Sample) { if (mission.CollectSample()) gameObject.SetActive(false); }
            else station.Adjust(Number);
        }
    }
}
