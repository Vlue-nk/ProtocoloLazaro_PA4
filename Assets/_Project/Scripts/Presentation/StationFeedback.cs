using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class StationFeedback : MonoBehaviour
    {
        [SerializeField] private AudioClip activatedSound;
        [SerializeField] private ParticleSystem sparks;
        private PowerStation station;
        private AudioSource source;
        void Awake()
        {
            station=GetComponent<PowerStation>();
            source=gameObject.AddComponent<AudioSource>();
            source.spatialBlend=1;
            source.minDistance=2;
            source.maxDistance=16;
        }
        void OnEnable()
        {
            station=GetComponent<PowerStation>();
            station.Activated+=Activate;
        }
        void OnDisable()
        {
            station.Activated-=Activate;
        }
        void Activate()
        {
            if(activatedSound)source.PlayOneShot(activatedSound);
            if(sparks)sparks.Play();
        }
    }
}
