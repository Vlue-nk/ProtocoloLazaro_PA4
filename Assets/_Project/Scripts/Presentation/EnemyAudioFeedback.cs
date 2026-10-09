using UnityEngine;
using UnityEngine.AI;

namespace ProtocoloLazaro
{
    [RequireComponent(typeof(ZombieAI), typeof(AudioSource))]
    public sealed class EnemyAudioFeedback : MonoBehaviour
    {
        [SerializeField] private AudioClip footstep;
        [SerializeField] private AudioClip attack;
        [SerializeField] private AudioClip death;
        private ZombieAI enemy;
        private NavMeshAgent agent;
        private AudioSource source;
        private float nextStep;

        private void Awake()
        {
            enemy = GetComponent<ZombieAI>();
            agent = GetComponent<NavMeshAgent>();
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 1;
            source.maxDistance = 14;
            source.volume = .6f;
        }

        private void OnEnable() => enemy.StateChanged += OnStateChanged;
        private void OnDisable() => enemy.StateChanged -= OnStateChanged;

        private void Update()
        {
            if (!GameManager.IsPlaying || enemy.IsDead || enemy.State == ZombieState.Stunned || !agent.enabled) return;
            float speed = agent.velocity.magnitude;
            if (speed < .2f || Time.time < nextStep) return;
            nextStep = Time.time + (speed > 2.5f ? .32f : .55f);
            if (footstep) source.PlayOneShot(footstep, .55f);
        }

        private void OnStateChanged(ZombieState state)
        {
            if (state == ZombieState.Stunned) source.Stop();
            else if (state == ZombieState.Attack && attack) source.PlayOneShot(attack);
            else if (state == ZombieState.Dead)
            {
                source.Stop();
                if (death) source.PlayOneShot(death);
            }
        }
    }
}
