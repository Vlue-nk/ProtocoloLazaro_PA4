using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class ExtractionDoor : MonoBehaviour
    {
        public bool IsUnlocked
        {
            get;
            private set;
        }
        public void Unlock()
        {
            IsUnlocked=true;
            var light=GetComponentInChildren<Light>();
            if(light)light.color=Color.green;
        }
        public void TryOpen(GameManager game)
        {
            var hp=FindFirstObjectByType<PlayerHealth>();
            if(IsUnlocked&&hp&&hp.Current>0&&GameManager.IsPlaying)game.Win();
        }
    }
}
