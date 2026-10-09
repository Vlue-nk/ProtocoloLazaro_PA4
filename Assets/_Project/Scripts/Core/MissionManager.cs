using System;
using System.Collections.Generic;
using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class MissionManager : MonoBehaviour
    {
        private readonly HashSet<PowerStation> stations=new();
        public int Activated=>stations.Count;
        public event Action<int,int> ProgressChanged;
        public void Activate(PowerStation station)
        {
            if(!GameManager.IsPlaying||!station||!station.IsActive||!stations.Add(station))return;
            ProgressChanged?.Invoke(Activated,3);
            if(Activated==3)FindFirstObjectByType<ExtractionDoor>()?.Unlock();
        }
    }
}
