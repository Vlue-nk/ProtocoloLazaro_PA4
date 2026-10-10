#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace ProtocoloLazaro
{
    public sealed class RouteValidation : MonoBehaviour
    {
        private PlayerController player;
        private CharacterController body;
        private Camera view;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!Environment.GetCommandLineArgs().Contains("--lazaro-route")) return;
            Application.runInBackground=true;
            var runner=new GameObject("Route validation");DontDestroyOnLoad(runner);runner.AddComponent<RouteValidation>();
        }
        static void Require(bool valid,string message) { if(!valid)throw new Exception("LAZARO_ROUTE_FAIL "+message);Debug.Log("LAZARO_ROUTE_PASS "+message); }
        static void FreezeThreats()
        {
            foreach(var enemy in FindObjectsByType<ZombieAI>(FindObjectsSortMode.None))
            {enemy.enabled=false;enemy.StopAllCoroutines();enemy.GetComponent<Collider>().enabled=false;var agent=enemy.GetComponent<NavMeshAgent>();if(agent.enabled&&agent.isOnNavMesh)agent.isStopped=true;}
        }
        void OnEnable() { Application.logMessageReceived += OnLog; }
        void OnDisable() { Application.logMessageReceived -= OnLog; }
        void OnLog(string condition,string trace,LogType type) { if(type==LogType.Exception && condition.Contains("LAZARO_ROUTE_FAIL"))Application.Quit(1); }
        IEnumerator Start()
        {
            yield return null;
            GameManager.Instance.Restart();yield return null;yield return null;
            player=FindFirstObjectByType<PlayerController>();body=player.GetComponent<CharacterController>();view=player.GetComponentInChildren<Camera>();player.enabled=false;
            FreezeThreats();
            var mission=FindFirstObjectByType<MissionManager>();
            var stations=FindObjectsByType<PowerStation>(FindObjectsSortMode.None).OrderBy(s=>s.Sector).ToArray();
            // Unopposed walking route: real CharacterController collision and interaction rays, no teleports.
            foreach(var station in stations)
            {
                var fuse=FindObjectsByType<MissionItem>(FindObjectsSortMode.None).Single(i=>i.Station==station&&i.Kind==MissionItemKind.Fuse);
                yield return WalkTo(fuse.transform);
                player.InteractFromView();Require(station.FuseCollected,"fuse raycast "+station.Sector);
                yield return WalkTo(station.transform);
                player.InteractFromView();Require(station.FuseInstalled,"install raycast "+station.Sector);
                Require(player.GetComponent<PulseEmitter>().TryPulse()&&station.IsCalibrating,"pulse raycast "+station.Sector);
                FreezeThreats();
                foreach(var kind in new[]{MissionItemKind.AdjustmentOne,MissionItemKind.AdjustmentTwo})
                {
                    var panel=FindObjectsByType<MissionItem>(FindObjectsSortMode.None).Single(i=>i.Station==station&&i.Kind==kind);
                    yield return WalkTo(panel.transform);
                    float deadline=Time.time+70;
                    while(station.PendingAdjustment==0&&Time.time<deadline)yield return null;
                    player.RefreshInteraction();yield return null;
                    string capture=Environment.GetEnvironmentVariable("LAZARO_ROUTE_CAPTURE");
                    if(!string.IsNullOrEmpty(capture) && station.Sector=="A" && kind==MissionItemKind.AdjustmentOne) { ScreenCapture.CaptureScreenshot(capture);yield return new WaitForSecondsRealtime(.3f); }
                    player.InteractFromView();Require(station.PendingAdjustment==0,"adjustment raycast "+station.Sector+" "+kind);
                }
                while(!station.IsActive)yield return null;
            }
            var sample=FindObjectsByType<MissionItem>(FindObjectsSortMode.None).Single(i=>i.Kind==MissionItemKind.Sample);
            yield return WalkTo(sample.transform);player.InteractFromView();Require(mission.HasSample,"sample raycast");
            var door=FindFirstObjectByType<ExtractionDoor>();yield return WalkTo(door.transform);player.InteractFromView();
            Require(GameManager.Instance.State==GameState.Won,"extraction raycast");
            Debug.Log("LAZARO_ROUTE_SECONDS "+mission.Elapsed.ToString("F2",System.Globalization.CultureInfo.InvariantCulture));
            Require(mission.Elapsed>=180&&mission.Elapsed<=420,"duration between 3 and 7 minutes");
            Debug.Log("LAZARO_ROUTE_COMPLETE");Application.Quit(0);
        }
        IEnumerator WalkTo(Transform target)
        {
            Require(NavMesh.SamplePosition(player.transform.position,out var start,2,NavMesh.AllAreas),"player on walkable floor");
            NavMeshPath path=null;float best=float.PositiveInfinity;
            foreach(var direction in new[]{Vector3.back,Vector3.forward,Vector3.left,Vector3.right})
            {
                var destination=target.position+direction*1.65f;destination.y=.1f;
                if(!NavMesh.SamplePosition(destination,out var end,.75f,NavMesh.AllAreas)||end.position.y>.5f)continue;
                var eye=end.position+Vector3.up*1.65f;var ray=target.position-eye;
                if(!Physics.Raycast(eye,ray.normalized,out var visible,2.5f,~(1<<8),QueryTriggerInteraction.Ignore)||!visible.transform.IsChildOf(target))continue;
                var candidate=new NavMeshPath();
                if(!NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,candidate)||candidate.status!=NavMeshPathStatus.PathComplete)continue;
                float length=0;for(int i=1;i<candidate.corners.Length;i++)length+=Vector3.Distance(candidate.corners[i-1],candidate.corners[i]);
                if(length<best){best=length;path=candidate;}
            }
            Require(path!=null,"reachable visible interaction point "+target.name);
            float deadline=Time.time+35;
            foreach(var corner in path.corners.Skip(1))
            {
                while(Vector3.Distance(new Vector3(player.transform.position.x,0,player.transform.position.z),new Vector3(corner.x,0,corner.z))>.15f)
                {
                    RequireSilent(Time.time<deadline,"collision prevented route to "+target.name);
                    var direction=corner-player.transform.position;direction.y=0;
                    body.Move(direction.normalized*Mathf.Min(4.5f*Time.deltaTime,direction.magnitude)+Vector3.down*2*Time.deltaTime);
                    view.transform.LookAt(target.position);
                    yield return null;
                }
            }
            view.transform.LookAt(target.position);yield return null;
            Physics.SyncTransforms();
            if(Physics.Raycast(view.transform.position,view.transform.forward,out var hit,3,~(1<<8),QueryTriggerInteraction.Ignore))Debug.Log("LAZARO_ROUTE_AIM "+target.name+" hit "+hit.collider.name+" distance "+hit.distance);
        }
        static void RequireSilent(bool condition,string message){if(!condition)throw new Exception("LAZARO_ROUTE_FAIL "+message);}
    }
}
#endif
