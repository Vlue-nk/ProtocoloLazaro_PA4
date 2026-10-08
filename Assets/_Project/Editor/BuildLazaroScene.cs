using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProtocoloLazaro;

public static class BuildLazaroScene
{
    [MenuItem("Lazaro/Build Laboratory Scene")]
    public static void Build()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single); scene.name="Laboratory_Main";
        var gm=new GameObject("GameManager");gm.AddComponent<GameManager>();
        var mission=new GameObject("MissionManager").AddComponent<MissionManager>();
        var root=new GameObject("LABORATORY");
        var floor=Mat("Floor",new Color(.07f,.1f,.14f)); var wall=Mat("Wall",new Color(.16f,.2f,.25f)); var cyan=Mat("StationInactive",new Color(.6f,.1f,.08f));
        Cube("Floor",new Vector3(0,-.5f,0),new Vector3(34,1,22),floor,root.transform);
        Cube("NorthWall",new Vector3(0,3,11),new Vector3(34,7,1),wall,root.transform);Cube("SouthWall",new Vector3(0,3,-11),new Vector3(34,7,1),wall,root.transform);Cube("WestWall",new Vector3(-17,3,0),new Vector3(1,7,22),wall,root.transform);Cube("EastWall",new Vector3(17,3,0),new Vector3(1,7,22),wall,root.transform);
        foreach(float x in new[]{-6f,5f})Cube("RoomDivider",new Vector3(x,3,0),new Vector3(.6f,7,14),wall,root.transform);
        for(int i=0;i<9;i++) {var p=new GameObject("Pipe");p.transform.position=new Vector3(-15+i*3,5.8f,9.8f);p.transform.localScale=new Vector3(2.5f,.15f,.15f);p.AddComponent<MeshFilter>().sharedMesh=Resources.GetBuiltinResource<Mesh>("Cube.fbx");p.AddComponent<MeshRenderer>().sharedMaterial=Mat("PipeMat",new Color(.15f,.45f,.55f));}
        for(int i=0;i<3;i++){var s=GameObject.CreatePrimitive(PrimitiveType.Cube);s.name="PowerStation_"+(i+1);s.transform.position=new Vector3(-11+i*11,1f,6);s.transform.localScale=new Vector3(1.2f,2,1.2f);s.GetComponent<Renderer>().sharedMaterial=cyan;s.AddComponent<PowerStation>();}
        var exit=GameObject.CreatePrimitive(PrimitiveType.Cube);exit.name="ExtractionDoor";exit.transform.position=new Vector3(14,2,0);exit.transform.localScale=new Vector3(.5f,4,5);exit.GetComponent<Renderer>().sharedMaterial=cyan;var ed=exit.AddComponent<ExtractionDoor>();
        var player=new GameObject("Player");player.tag="Player";player.layer=LayerMask.NameToLayer("Default");player.transform.position=new Vector3(-13,1,0);var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.radius=.35f;var ph=player.AddComponent<PlayerHealth>();var camgo=new GameObject("PlayerCamera");camgo.transform.SetParent(player.transform);camgo.transform.localPosition=new Vector3(0,.65f,0);var cam=camgo.AddComponent<Camera>();cam.fieldOfView=72;var pc=player.AddComponent<PlayerController>();var weapon=player.AddComponent<WeaponRaycast>();var pulse=player.AddComponent<PulseEmitter>();
        for(int i=0;i<4;i++){var z=GameObject.CreatePrimitive(PrimitiveType.Capsule);z.name="Infected_"+(i+1);z.transform.position=new Vector3(-8+i*7,1,(i%2==0?3:-3));z.transform.localScale=new Vector3(.8f,1.1f,.8f);z.GetComponent<Renderer>().sharedMaterial=Mat("InfectedMat",new Color(.45f,.7f,.35f));var a=z.AddComponent<NavMeshAgent>();a.speed=1.8f;a.stoppingDistance=1.2f;z.AddComponent<ZombieAI>();}
        var canvas=new GameObject("HUD",typeof(Canvas),typeof(CanvasScaler));canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;canvas.GetComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;canvas.GetComponent<CanvasScaler>().referenceResolution=new Vector2(1920,1080);var hud=canvas.AddComponent<HUDController>();
        Text t1=Text(canvas.transform,"HP: 100/100",new Vector2(180,60),new Vector2(130,-60),32);Text t2=Text(canvas.transform,"AMMO: 6/6",new Vector2(220,60),new Vector2(-150,-60),32);Text t3=Text(canvas.transform,"ESTACIONES: 0/3",new Vector2(500,60),new Vector2(0,-45),28);Text t4=Text(canvas.transform,"PULSO: LISTO [Q]",new Vector2(360,60),new Vector2(0,45),24);Text info=Text(canvas.transform,"WASD mover | Shift correr | Click disparar | R recargar | Q pulso | Esc pausa",new Vector2(1000,50),new Vector2(0,250),20);info.alignment=TextAnchor.MiddleCenter;
        var nav=root.AddComponent<NavMeshSurface>();nav.collectObjects=CollectObjects.All;nav.BuildNavMesh();EditorSceneManager.SaveScene(scene,"Assets/_Project/Scenes/Laboratory_Main.unity");AssetDatabase.SaveAssets();
    }
    static Material Mat(string n,Color c){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=n,color=c};return m;}
    static void Cube(string n,Vector3 p,Vector3 s,Material m,Transform parent){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=n;o.transform.SetParent(parent);o.transform.position=p;o.transform.localScale=s;o.GetComponent<Renderer>().sharedMaterial=m;}
    static Text Text(Transform p,string value,Vector2 size,Vector2 pos,int fs){var go=new GameObject(value);go.transform.SetParent(p,false);var r=go.AddComponent<RectTransform>();r.sizeDelta=size;r.anchoredPosition=pos;var t=go.AddComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=fs;t.color=Color.white;return t;}
}
