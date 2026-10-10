using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Profiling;
namespace ProtocoloLazaro
{
    [RequireComponent(typeof(NavMeshAgent))] public sealed class ZombieAI : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float health=100,sightRange=12,halfSightAngle=55,attackRange=1.5f;
        private NavMeshAgent agent;
        private PlayerHealth player;
        private Animator animator;
        private Vector3 lastKnown;
        private float stunEnd,nextAttack,strikeAt,searchEnd,patrolWait;
        private int waypoint;
        private bool strikePending;
        private static readonly ProfilerMarker Perception=new("Lazaro.Perception");
        public ZombieState State
        {
            get;
            private set;
        }
        =ZombieState.Patrol;
        public bool IsDead=>State==ZombieState.Dead;
        public event System.Action<ZombieState> StateChanged;
        void Awake()
        {
            agent=GetComponent<NavMeshAgent>();
            animator=GetComponentInChildren<Animator>();
        }
        void Start()
        {
            // Scene agents are enabled only after NavMeshSurface.OnEnable has registered its data.
            agent.enabled=true;
            if(!agent.isOnNavMesh)
            {
                Debug.LogError("Infected could not attach to the saved NavMesh: "+name, this);
                enabled=false;
                return;
            }
            player=FindFirstObjectByType<PlayerHealth>();
            StartCoroutine(SenseLoop());
        }
        IEnumerator SenseLoop()
        {
            var wait=new WaitForSeconds(.2f);
            while(!IsDead)
            {
                if(GameManager.IsPlaying)Sense();
                yield return wait;
            }
        }
        void Sense()
        {
            using(Perception.Auto())
            {
                if(!agent.isOnNavMesh||!player)return;
                if(State==ZombieState.Stunned)
                {
                    if(Time.time<stunEnd)return;
                    SetState(ZombieState.Investigate);
                    searchEnd=0;
                }
                if(State==ZombieState.Attack)return;
                if(CanSee())
                {
                    lastKnown=player.transform.position;
                    if(Vector3.Distance(transform.position,lastKnown)<=attackRange)
                    {
                        if(Time.time>=nextAttack)
                        {
                            SetState(ZombieState.Attack);
                            strikeAt=Time.time+.4f;
                            nextAttack=Time.time+1.2f;
                            strikePending=true;
                        }
                        else agent.isStopped=true;
                        return;
                    }
                    SetState(ZombieState.Chase);
                    Move(lastKnown,4.2f);
                    return;
                }
                if(State==ZombieState.Chase)
                {
                    SetState(ZombieState.Investigate);
                    searchEnd=0;
                }
                if(State==ZombieState.Investigate)
                {
                    Move(lastKnown,1.8f);
                    if(!agent.pathPending&&agent.remainingDistance<=1)
                    {
                        agent.isStopped=true;
                        if(searchEnd==0)searchEnd=Time.time+4;
                        transform.Rotate(0,25*.2f,0);
                        if(Time.time>=searchEnd)
                        {
                            SetState(ZombieState.Patrol);
                            searchEnd=0;
                        }
                    }
                    return;
                }
                if(waypoints==null||waypoints.Length==0)
                {
                    agent.isStopped=true;
                    return;
                }
                Move(waypoints[waypoint].position,1.8f);
                if(!agent.pathPending&&agent.remainingDistance<=.5f)
                {
                    agent.isStopped=true;
                    if(patrolWait==0)patrolWait=Time.time+2;
                    if(Time.time>=patrolWait)
                    {
                        waypoint=(waypoint+1)%waypoints.Length;
                        patrolWait=0;
                    }
                }
            }
        }
        void Update()
        {
            if(!GameManager.IsPlaying||IsDead)return;
            if(strikePending&&Time.time>=strikeAt)
            {
                strikePending=false;
                if(State==ZombieState.Attack&&player&&Vector3.Distance(transform.position,player.transform.position)<=attackRange&&ClearLine())player.Damage(25);
                if(State==ZombieState.Attack)SetState(ZombieState.Chase);
            }
            if(animator)
            {
                animator.speed=State==ZombieState.Stunned?0:1;
                animator.SetFloat("Speed",agent.enabled&&agent.isOnNavMesh?agent.velocity.magnitude:0);
            }
        }
        bool ClearLine()
        {
            return player&&!Physics.Linecast(transform.position+Vector3.up*1.3f,player.transform.position+Vector3.up,1<<10,QueryTriggerInteraction.Ignore);
        }
        bool CanSee()
        {
            if(!player)return false;
            Vector3 d=player.transform.position-transform.position;
            return d.sqrMagnitude<=sightRange*sightRange&&Vector3.Angle(transform.forward,d)<=halfSightAngle&&ClearLine();
        }
        void Move(Vector3 destination,float speed)
        {
            if(!agent.enabled||!agent.isOnNavMesh)return;
            agent.isStopped=false;
            agent.speed=speed;
            agent.stoppingDistance=State==ZombieState.Patrol?.25f:1.05f;
            if(NavMesh.SamplePosition(destination,out var hit,2,NavMesh.AllAreas))agent.SetDestination(hit.position);
        }
        void SetState(ZombieState next)
        {
            if(State==next)return;
            State=next;
            strikePending=false;
            if(agent.enabled&&agent.isOnNavMesh)agent.isStopped=next==ZombieState.Attack||next==ZombieState.Stunned||next==ZombieState.Dead;
            if(animator&&next==ZombieState.Attack)animator.SetTrigger("Attack");
            StateChanged?.Invoke(next);
        }
        public void Hear(Vector3 position)
        {
            if(IsDead)return;
            lastKnown=position;
            if(State==ZombieState.Stunned||State==ZombieState.Attack||State==ZombieState.Chase)return;
            SetState(ZombieState.Investigate);
            searchEnd=0;
            Move(lastKnown,1.8f);
        }
        public void Stun(float seconds)
        {
            if(IsDead)return;
            SetState(ZombieState.Stunned);
            stunEnd=Time.time+seconds;
            strikePending=false;
            if(animator)animator.speed=0;
        }
        public void Damage(float amount)
        {
            if(IsDead||!GameManager.IsPlaying)return;
            health-=amount;
            if(health>0)
            {
                if(player)Hear(player.transform.position);
                return;
            }
            SetState(ZombieState.Dead);
            agent.enabled=false;
            GetComponent<Collider>().enabled=false;
            if(animator)
            {
                animator.speed=1;
                animator.SetTrigger("Die");
            }
        }
    }
}
