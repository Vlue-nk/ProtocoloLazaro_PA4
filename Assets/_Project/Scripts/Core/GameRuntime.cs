using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProtocoloLazaro
{
    public enum GameState { Playing, Paused, Won, Lost }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.Playing;
        public event Action<GameState> StateChanged;
        [SerializeField] private string gameplayScene = "Laboratory_Main";
        private void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; Time.timeScale = 1f; }
        public void Pause() { if (State != GameState.Playing) return; State = GameState.Paused; Time.timeScale = 0f; StateChanged?.Invoke(State); }
        public void Resume() { if (State != GameState.Paused) return; Time.timeScale = 1f; State = GameState.Playing; StateChanged?.Invoke(State); }
        public void Win() { if (State != GameState.Playing) return; State = GameState.Won; StateChanged?.Invoke(State); }
        public void Lose() { if (State != GameState.Playing) return; State = GameState.Lost; StateChanged?.Invoke(State); }
        public void Restart() { Time.timeScale = 1f; SceneManager.LoadScene(gameplayScene); }
        public void Menu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    }

    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterController controller; [SerializeField] private Transform cameraRoot; [SerializeField] private float walkSpeed=4.5f, runSpeed=7f, mouseSensitivity=2.2f;
        private float pitch; private void Start(){ if(!controller) controller=GetComponent<CharacterController>(); if(!cameraRoot)cameraRoot=GetComponentInChildren<Camera>()?.transform; Cursor.lockState=CursorLockMode.Locked; Cursor.visible=false; }
        private void Update(){ if(GameManager.Instance && GameManager.Instance.State!=GameState.Playing)return; float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical"); Vector3 move=(transform.right*x+transform.forward*z).normalized*(Input.GetKey(KeyCode.LeftShift)?runSpeed:walkSpeed); if(!controller.isGrounded)move.y=-9.81f; controller.Move(move*Time.deltaTime); float mx=Input.GetAxis("Mouse X")*mouseSensitivity, my=Input.GetAxis("Mouse Y")*mouseSensitivity; transform.Rotate(Vector3.up*mx); pitch=Mathf.Clamp(pitch-my,-80,80); cameraRoot.localRotation=Quaternion.Euler(pitch,0,0); if(Input.GetKeyDown(KeyCode.Escape)){if(GameManager.Instance.State==GameState.Paused)GameManager.Instance.Resume();else GameManager.Instance.Pause();} if(Input.GetKeyDown(KeyCode.E))Interact(); }
        private void Interact(){var cam=cameraRoot?cameraRoot.GetComponent<Camera>():null;if(!cam)return;if(Physics.Raycast(cam.transform.position,cam.transform.forward,out var hit,3f,~0,QueryTriggerInteraction.Ignore)){var door=hit.collider.GetComponentInParent<ExtractionDoor>();if(door)door.TryOpen(GameManager.Instance);var med=hit.collider.GetComponentInParent<Medkit>();if(med)med.Use(GetComponent<PlayerHealth>());}}
    }

    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth=100; public int Current {get; private set;} public event Action<int,int> HealthChanged; public event Action Died;
        private void Awake(){Current=maxHealth;} public void Damage(int amount){if(GameManager.Instance.State!=GameState.Playing)return;Current=Mathf.Max(0,Current-amount);HealthChanged?.Invoke(Current,maxHealth);if(Current==0){Died?.Invoke();GameManager.Instance.Lose();}}
        public void Heal(int amount){Current=Mathf.Min(maxHealth,Current+amount);HealthChanged?.Invoke(Current,maxHealth);}
    }

    public sealed class WeaponRaycast : MonoBehaviour
    {
        [SerializeField] private Camera cam; [SerializeField] private PlayerHealth health; [SerializeField] private int magazine=6,reserve=6,damage=35; [SerializeField] private float interval=.4f, range=25f; public int Magazine=>magazine; public int Reserve=>reserve; public event Action<int,int> AmmoChanged; public event Action<Vector3,float> Noise;
        private float nextShot; private bool reloading; private void Start(){if(!cam)cam=GetComponentInChildren<Camera>();if(!health)health=GetComponent<PlayerHealth>();}
        private void Update(){if(GameManager.Instance.State!=GameState.Playing)return;if(Input.GetMouseButtonDown(0)&&Time.time>=nextShot&&!reloading)Shoot();if(Input.GetKeyDown(KeyCode.R)&&!reloading&&magazine<6&&reserve>0)StartCoroutine(Reload());}
        private void Shoot(){if(magazine<=0){StartCoroutine(Reload());return;}magazine--;nextShot=Time.time+interval;AmmoChanged?.Invoke(magazine,reserve);Noise?.Invoke(transform.position,10f);if(Physics.Raycast(cam.transform.position,cam.transform.forward,out var hit,range,~0,QueryTriggerInteraction.Ignore)){var z=hit.collider.GetComponentInParent<ZombieAI>();if(z)z.Damage(damage);}}
        private IEnumerator Reload(){reloading=true;yield return new WaitForSeconds(1.5f);int n=Mathf.Min(6-magazine,reserve);magazine+=n;reserve-=n;AmmoChanged?.Invoke(magazine,reserve);reloading=false;}
    }

    public sealed class MissionManager : MonoBehaviour
    {
        public int Activated {get; private set;} public event Action<int,int> ProgressChanged; [SerializeField] private ExtractionDoor extraction;
        private readonly HashSet<PowerStation> done=new(); private void Start(){if(!extraction)extraction=FindFirstObjectByType<ExtractionDoor>();} public void Activate(PowerStation station){if(done.Add(station)){Activated++;ProgressChanged?.Invoke(Activated,3);if(Activated>=3&&extraction)extraction.Unlock();}}
    }
    public sealed class PowerStation : MonoBehaviour
    { [SerializeField] private Renderer display; private bool active; public void Activate(MissionManager m){if(active)return;active=true;if(display)display.material.color=Color.green;m.Activate(this);} }
    public sealed class ExtractionDoor : MonoBehaviour
    { private bool unlocked; public void Unlock(){unlocked=true;var r=GetComponent<Renderer>();if(r)r.material.color=Color.green;} public void TryOpen(GameManager gm){if(unlocked)gm.Win();} }
    public sealed class Medkit : MonoBehaviour
    { [SerializeField] private int amount=30; private bool used; public void Use(PlayerHealth target){if(used||!target||target.Current>=100)return;used=true;target.Heal(amount);gameObject.SetActive(false);} }

    public sealed class PulseEmitter : MonoBehaviour
    {
        [SerializeField] private Camera cam; [SerializeField] private MissionManager mission; [SerializeField] private float cooldown=8f, radius=4.5f, stun=3f; private float ready; private readonly Collider[] buffer=new Collider[16]; public event Action<float> CooldownChanged; public event Action<Vector3,float> Noise;
        private void Start(){if(!cam)cam=GetComponentInChildren<Camera>();if(!mission)mission=FindFirstObjectByType<MissionManager>();}
        private void Update(){if(GameManager.Instance.State!=GameState.Playing)return;if(Input.GetKeyDown(KeyCode.Q)&&Time.time>=ready)Pulse();if(Time.time<ready)CooldownChanged?.Invoke(Mathf.Clamp01((ready-Time.time)/cooldown));}
        private void Pulse(){ready=Time.time+cooldown;Noise?.Invoke(transform.position,18f);int n=Physics.OverlapSphereNonAlloc(transform.position,radius,buffer,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<n;i++){if(buffer[i].TryGetComponent<ZombieAI>(out var z))z.Stun(stun);if(buffer[i].TryGetComponent<PowerStation>(out var p)){if(Physics.Raycast(cam.transform.position,cam.transform.forward,out var h,3f)&&h.collider.GetComponentInParent<PowerStation>()==p)mission.Activate(p);}}}
    }

    public enum ZombieState { Patrol, Investigate, Chase, Attack, Stunned, Dead }
    public sealed class ZombieAI : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent; [SerializeField] private Transform player; [SerializeField] private PlayerHealth playerHealth; [SerializeField] private float health=100, sightRange=12, sightAngle=55, attackRange=1.5f; private ZombieState state=ZombieState.Patrol; private Vector3 lastNoise; private float stunEnd, attackEnd; private int waypoint; [SerializeField] private Transform[] waypoints; private Animator animator;
        private void Awake(){agent??=GetComponent<NavMeshAgent>();animator=GetComponentInChildren<Animator>();}
        private void Start(){if(!player){var p=FindFirstObjectByType<PlayerController>();if(p)player=p.transform;}playerHealth=FindFirstObjectByType<PlayerHealth>();var w=FindFirstObjectByType<WeaponRaycast>();if(w)w.Noise+=HearNearby;var pe=FindFirstObjectByType<PulseEmitter>();if(pe)pe.Noise+=HearNearby;StartCoroutine(SenseLoop());}
        private void HearNearby(Vector3 p,float radius){if(Vector3.Distance(transform.position,p)<=radius)Hear(p);}
        private IEnumerator SenseLoop(){var wait=new WaitForSeconds(.2f);while(state!=ZombieState.Dead){if(GameManager.Instance.State==GameState.Playing){if(state==ZombieState.Stunned){if(Time.time>=stunEnd)state=ZombieState.Investigate;}else if(CanSeePlayer())state=ZombieState.Chase;else if(state==ZombieState.Chase)state=ZombieState.Investigate;UpdateState();}yield return wait;}}
        private bool CanSeePlayer(){if(!player)return false;Vector3 d=player.position-transform.position;if(d.magnitude>sightRange||Vector3.Angle(transform.forward,d)>sightAngle)return false;return !Physics.Raycast(transform.position+Vector3.up*1.2f,d.normalized,d.magnitude,~0,QueryTriggerInteraction.Ignore)||Physics.Raycast(transform.position+Vector3.up*1.2f,d.normalized,out var h,d.magnitude,~0,QueryTriggerInteraction.Ignore)&&h.collider.GetComponentInParent<PlayerController>();}
        private void UpdateState(){if(state==ZombieState.Stunned||state==ZombieState.Dead)return;if(state==ZombieState.Chase){agent.speed=4.2f;agent.SetDestination(player.position);if(Vector3.Distance(transform.position,player.position)<=attackRange)Attack();}else{agent.speed=1.8f;if(waypoints!=null&&waypoints.Length>0){agent.SetDestination(waypoints[waypoint].position);if(!agent.pathPending&&agent.remainingDistance<.5f)waypoint=(waypoint+1)%waypoints.Length;}}}
        private void Attack(){state=ZombieState.Attack;agent.isStopped=true;if(Time.time>=attackEnd){attackEnd=Time.time+1.2f;StartCoroutine(Strike());}}
        private IEnumerator Strike(){yield return new WaitForSeconds(.4f);if(state==ZombieState.Attack&&player&&Vector3.Distance(transform.position,player.position)<=attackRange&&CanSeePlayer())playerHealth.Damage(25);if(state==ZombieState.Attack){state=ZombieState.Chase;agent.isStopped=false;}}
        public void Hear(Vector3 p){if(state==ZombieState.Dead||state==ZombieState.Stunned)return;lastNoise=p;state=ZombieState.Investigate;agent.SetDestination(p);}
        public void Stun(float duration){if(state==ZombieState.Dead)return;state=ZombieState.Stunned;stunEnd=Time.time+duration;agent.isStopped=true;}
        public void Damage(float amount){health-=amount;if(health<=0){state=ZombieState.Dead;agent.isStopped=true;agent.enabled=false;var c=GetComponent<Collider>();if(c)c.enabled=false;if(animator)animator.SetTrigger("Die");} }
    }

    public sealed class HUDController : MonoBehaviour
    { [SerializeField] private Text hp,ammo,objective,pulse; [SerializeField] private PlayerHealth health; [SerializeField] private WeaponRaycast weapon; [SerializeField] private MissionManager mission; [SerializeField] private PulseEmitter emitter; private void Start(){if(!health)health=FindFirstObjectByType<PlayerHealth>();if(!weapon)weapon=FindFirstObjectByType<WeaponRaycast>();if(!mission)mission=FindFirstObjectByType<MissionManager>();if(!emitter)emitter=FindFirstObjectByType<PulseEmitter>();foreach(var t in GetComponentsInChildren<Text>(true)){if(t.name.Contains("HP"))hp??=t;else if(t.name.Contains("AMMO"))ammo??=t;else if(t.name.Contains("ESTACIONES"))objective??=t;else if(t.name.Contains("PULSO"))pulse??=t;}if(health)health.HealthChanged+=SetHealth;if(weapon)weapon.AmmoChanged+=SetAmmo;if(mission)mission.ProgressChanged+=SetMission;if(emitter)emitter.CooldownChanged+=SetPulse;SetHealth(100,100);SetAmmo(6,6);SetMission(mission?mission.Activated:0,3);SetPulse(0);} private void OnDisable(){if(health)health.HealthChanged-=SetHealth;if(weapon)weapon.AmmoChanged-=SetAmmo;if(mission)mission.ProgressChanged-=SetMission;if(emitter)emitter.CooldownChanged-=SetPulse;} private void SetHealth(int a,int b){if(hp)hp.text=$"HP: {a}/{b}";} private void SetAmmo(int a,int b){if(ammo)ammo.text=$"AMMO: {a}/{b}";} private void SetMission(int a,int b){if(objective)objective.text=$"ESTACIONES: {a}/{b}";} private void SetPulse(float v){if(pulse)pulse.text=v<=0?"PULSO: LISTO [Q]":$"PULSO: {Mathf.CeilToInt(v*8)}s";}
    }
}
