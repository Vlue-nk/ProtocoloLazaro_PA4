using System;
using TMPro;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Unity.AI.Navigation;
using ProtocoloLazaro;

public static class BuildLazaroScene
{
 const string Base="Assets/_Project/";
 static Material wall,floor,trim,cyan,red,white,green;
 static readonly Color Ink=new(.025f,.055f,.075f), Accent=new(.12f,.9f,.82f);
 [MenuItem("Lazaro/Rebuild Complete Game")]
 public static void Build()
 {
  EditorSettings.serializationMode=SerializationMode.ForceText;
  foreach(var dir in new[]{"Materials","Animations","Prefabs","Scenes","Settings"})Directory.CreateDirectory(Base+dir);
  var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);var layers=tags.FindProperty("layers");layers.GetArrayElementAtIndex(8).stringValue="Player";layers.GetArrayElementAtIndex(9).stringValue="Enemy";layers.GetArrayElementAtIndex(10).stringValue="World";tags.ApplyModifiedPropertiesWithoutUndo();
  wall=Mat("Architecture",new Color(.16f,.22f,.28f));floor=Mat("Floor",new Color(.08f,.12f,.17f));trim=Mat("Trim",new Color(.035f,.06f,.09f));cyan=Mat("Cyan",Accent,2);red=Mat("Inactive",new Color(.9f,.12f,.08f),1.5f);green=Mat("Medical",new Color(.2f,.95f,.5f),1);white=Mat("White",new Color(.65f,.74f,.8f));
  var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/ThirdParty/Kenney/GraveyardKit/Models/character-zombie.fbx");
  importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=true;importer.optimizeGameObjects=false;
  var clips=importer.defaultClipAnimations;foreach(var clip in clips)clip.loopTime=clip.name=="idle"||clip.name=="walk"||clip.name=="sprint";importer.clipAnimations=clips;importer.SaveAndReimport();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("GameManager").AddComponent<GameManager>();new GameObject("Mission").AddComponent<MissionManager>();
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.3f,.38f,.48f);RenderSettings.fog=true;RenderSettings.fogColor=Ink;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=18;RenderSettings.fogEndDistance=55;
  var sun=new GameObject("Soft overhead").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(.55f,.72f,.9f);sun.intensity=.8f;sun.transform.rotation=Quaternion.Euler(70,-30,0);sun.shadows=LightShadows.Soft;
  var root=new GameObject("Laboratory Geometry");
  var roof=new GameObject("Ceiling - hide in Scene view to inspect interior");
  Cube("Ceiling",new Vector3(0,5.3f,0),new Vector3(40,.3f,30),trim,roof.transform);
  for(int x=-18;x<=18;x+=4){Cube("Wall column",new Vector3(x,2.4f,14.6f),new Vector3(.25f,4.8f,.3f),white,root.transform);Cube("Wall column",new Vector3(x,2.4f,-14.6f),new Vector3(.25f,4.8f,.3f),white,root.transform);}
  Cube("North guide",new Vector3(0,.35f,14.7f),new Vector3(39,.08f,.06f),cyan,root.transform);Cube("South guide",new Vector3(0,.35f,-14.7f),new Vector3(39,.08f,.06f),cyan,root.transform);
  Cube("Foundation",new Vector3(0,-.3f,0),new Vector3(40,.6f,30),floor,root.transform);
  Cube("North",new Vector3(0,2.5f,15),new Vector3(40,5,.4f),wall,root.transform);Cube("South",new Vector3(0,2.5f,-15),new Vector3(40,5,.4f),wall,root.transform);Cube("West",new Vector3(-20,2.5f,0),new Vector3(.4f,5,30),wall,root.transform);Cube("East",new Vector3(20,2.5f,0),new Vector3(.4f,5,30),wall,root.transform);
  // A central spine and two transverse barriers make a loop with two broad accesses per room.
  Cube("Spine",new Vector3(0,2.5f,0),new Vector3(.5f,5,22),wall,root.transform);
  Cube("West partition",new Vector3(-10,2.5f,0),new Vector3(12,5,.5f),wall,root.transform);Cube("East partition",new Vector3(10,2.5f,0),new Vector3(12,5,.5f),wall,root.transform);
  for(int x=-18;x<=18;x+=4)for(int z=-13;z<=13;z+=4){Cube("Floor joint",new Vector3(x,.005f,z),new Vector3(3.95f,.015f,.035f),trim,root.transform);}
  foreach(var p in new[]{new Vector3(-10,0,-8),new Vector3(-10,0,8),new Vector3(10,0,8),new Vector3(10,0,-8)})
  {
   Point(p+Vector3.up*4,new Color(.25f,.7f,1),4,14);
   for(int i=0;i<3;i++){Cube("Ceiling luminaire",p+new Vector3(-4+i*4,4.8f,0),new Vector3(2,.08f,.18f),cyan,root.transform);}
   var table=Model("SpaceStationKit","table-large",p+new Vector3(2,0,0),3);BoxFor(table);
   var console=Model("SpaceStationKit","computer-system",p+new Vector3(-5,0,3),1.5f);BoxFor(console);
   var pipe=Model("SpaceStationKit","pipe",p+new Vector3(-5,4.4f,5),3);pipe.transform.rotation=Quaternion.Euler(0,0,90);
   var crate=Model("SpaceStationKit","container-tall",p+new Vector3(5,0,-3),1.5f);BoxFor(crate);
  }
  Sign("01 / RECEPCIÓN",new Vector3(-10,3,-14.7f));Sign("02 / CULTIVO",new Vector3(-10,3,14.7f),180);Sign("03 / GENERADORES",new Vector3(10,3,14.7f),180);Sign("04 / CONTENCIÓN",new Vector3(10,3,-14.7f));
  var stationPositions=new[]{new Vector3(-13,0,10),new Vector3(12,0,10),new Vector3(12,0,-10)};
  for(int i=0;i<3;i++)
  {
   var s=Cube("Station "+(i+1),stationPositions[i]+Vector3.up*.65f,new Vector3(1.6f,1.3f,1),trim);s.transform.position=stationPositions[i]+Vector3.up*.65f;
   var display=Cube("Status",s.transform.position+new Vector3(0,.85f,0),new Vector3(1.4f,.45f,.6f),red,s.transform);
   Model("SpaceStationKit","computer-screen",s.transform.position+new Vector3(0,1.1f,0),1.2f,s.transform);
   var beacon=Point(s.transform.position+Vector3.up*2,Color.red,2,5);beacon.transform.SetParent(s.transform);
   var station=s.AddComponent<PowerStation>();Set(station,"display",display.GetComponent<Renderer>());Set(station,"beacon",beacon);
   var fx=s.AddComponent<StationFeedback>();Set(fx,"activatedSound",Clip("SciFiSounds","computerNoise_000"));Set(fx,"sparks",Particles("Station sparks",s.transform.position+Vector3.up*1.5f,Color.green,2,s.transform));
   Sign("ESTACIÓN "+(char)('A'+i),s.transform.position+new Vector3(0,2.3f,0));PrefabUtility.SaveAsPrefabAsset(s,Base+"Prefabs/Station"+(i+1)+".prefab");
  }
  var exit=Cube("Extraction",new Vector3(-14,1.7f,-13.8f),new Vector3(3,3.4f,.4f),trim);exit.AddComponent<ExtractionDoor>();var el=Point(exit.transform.position+new Vector3(0,2,-.2f),Color.red,3,5);el.transform.SetParent(exit.transform);Sign("EXTRACCIÓN / 3 ESTACIONES",new Vector3(-14,3.8f,-13.5f));
  var med=Cube("Medkit",new Vector3(-5,.6f,-9),new Vector3(.7f,.5f,.45f),white);med.AddComponent<Medkit>();Cube("Medical stripe",med.transform.position+new Vector3(0,.26f,0),new Vector3(.45f,.02f,.12f),green,med.transform);
  var player=new GameObject("Player");player.tag="Player";player.layer=8;player.transform.position=new Vector3(-14,.1f,-9);var cc=player.AddComponent<CharacterController>();cc.height=1.8f;cc.center=new Vector3(0,.9f,0);cc.radius=.3f;cc.stepOffset=.25f;
  var camera=new GameObject("PlayerCamera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.SetParent(player.transform,false);camera.transform.localPosition=new Vector3(0,1.65f,0);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;camera.fieldOfView=72;camera.nearClipPlane=.04f;camera.gameObject.AddComponent<AudioListener>();camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
  player.AddComponent<PlayerController>();player.AddComponent<PlayerHealth>();player.AddComponent<WeaponRaycast>();player.AddComponent<PulseEmitter>();
  var weapon=Model("BlasterKit","blaster-a",Vector3.zero,.45f);weapon.transform.SetParent(camera.transform,false);weapon.transform.localPosition=new Vector3(.28f,-.23f,.48f);weapon.transform.localRotation=Quaternion.Euler(0,180,0);
  var feedback=player.AddComponent<PlayerFeedback>();Set(feedback,"weaponVisual",weapon.transform);Set(feedback,"shot",Clip("SciFiSounds","laserSmall_000"));Set(feedback,"pulseSound",Clip("SciFiSounds","forceField_000"));Set(feedback,"hurt",Clip("ImpactSounds","impactPunch_medium_000"));Set(feedback,"footstep",Clip("ImpactSounds","footstep_concrete_000"));Set(feedback,"pulseEffect",Particles("Resonance pulse",player.transform.position+Vector3.up,Accent,8,player.transform));
  PrefabUtility.SaveAsPrefabAsset(player,Base+"Prefabs/Player.prefab");
  new GameObject("NoiseSystem").AddComponent<NoiseSystem>();
  // Bake only static world geometry, then persist navigation as a project asset.
  var surface=root.AddComponent<NavMeshSurface>();surface.collectObjects=CollectObjects.All;surface.layerMask=1<<10;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.BuildNavMesh();
  if(surface.navMeshData){var old=AssetDatabase.LoadAssetAtPath<NavMeshData>(Base+"Settings/LaboratoryNavMesh.asset");if(old){EditorUtility.CopySerialized(surface.navMeshData,old);surface.RemoveData();surface.navMeshData=old;surface.AddData();}else AssetDatabase.CreateAsset(surface.navMeshData,Base+"Settings/LaboratoryNavMesh.asset");}
  var controller=ZombieAnimator();
  var starts=new[]{new Vector3(-10,0,7),new Vector3(10,0,6),new Vector3(6,0,-7),new Vector3(15,0,-6)};
  for(int i=0;i<4;i++)
  {
   var z=new GameObject("Infected "+(i+1));z.layer=9;z.transform.position=starts[i];var body=z.AddComponent<CapsuleCollider>();body.height=1.8f;body.center=Vector3.up*.9f;body.radius=.35f;
   var model=Model("GraveyardKit","character-zombie",starts[i],1.8f,z.transform);var animator=model.GetComponent<Animator>();if(!animator)animator=model.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.applyRootMotion=false;
   var agent=z.AddComponent<NavMeshAgent>();agent.radius=.35f;agent.height=1.8f;agent.speed=1.8f;agent.stoppingDistance=1.05f;agent.angularSpeed=240;var ai=z.AddComponent<ZombieAI>();
   var points=new Transform[3];for(int j=0;j<3;j++){points[j]=new GameObject("Patrol "+i+"-"+j).transform;var candidate=starts[i]+new Vector3(j==1?2:-2,0,j==2?3:-2);points[j].position=NavMesh.SamplePosition(candidate,out var h,3,NavMesh.AllAreas)?h.position:starts[i];}
   var so=new SerializedObject(ai);var arr=so.FindProperty("waypoints");arr.arraySize=3;for(int j=0;j<3;j++)arr.GetArrayElementAtIndex(j).objectReferenceValue=points[j];so.ApplyModifiedPropertiesWithoutUndo();
  }
  var ambient=new GameObject("Ambient machinery").AddComponent<AudioSource>();ambient.clip=Clip("SciFiSounds","spaceEngineLow_000");ambient.loop=true;ambient.volume=.18f;ambient.playOnAwake=true;
  var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Base+"Settings/Atmosphere.asset");if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,Base+"Settings/Atmosphere.asset");var bloom=profile.Add<Bloom>();bloom.intensity.Override(.25f);var vignette=profile.Add<Vignette>();vignette.intensity.Override(.18f);foreach(var component in profile.components)AssetDatabase.AddObjectToAsset(component,profile);}
  var volume=new GameObject("Atmosphere").AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=profile;
  GameplayUI();EditorSceneManager.SaveScene(scene,Base+"Scenes/Laboratory_Main.unity");
  MainMenu();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Base+"Scenes/MainMenu.unity",true),new EditorBuildSettingsScene(Base+"Scenes/Laboratory_Main.unity",true)};
  PlayerSettings.productName="Protocolo Lázaro";PlayerSettings.companyName="ProtocoloLazaro";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();Validate();Debug.Log("LAZARO_SCENES_VALIDATED");
 }
 static void Set(UnityEngine.Object target,string name,UnityEngine.Object value){var s=new SerializedObject(target);s.FindProperty(name).objectReferenceValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
 static Material Mat(string name,Color color,float emission=0){var path=Base+"Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}return m;}
 static GameObject Cube(string name,Vector3 pos,Vector3 scale,Material material,Transform parent=null){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.layer=10;g.transform.SetParent(parent,true);g.transform.position=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=material;return g;}
 static Light Point(Vector3 p,Color c,float strength,float radius){var light=new GameObject("Beacon").AddComponent<Light>();light.transform.position=p;light.color=c;light.intensity=strength;light.range=radius;return light;}
 static AudioClip Clip(string pack,string name)=>AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/ThirdParty/Kenney/"+pack+"/Audio/"+name+".ogg");
 static GameObject Model(string pack,string name,Vector3 position,float height,Transform parent=null)
 {
  string path="Assets/ThirdParty/Kenney/"+pack+"/Models/"+name+".fbx";var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!source)throw new Exception("Missing asset: "+path);
  var g=(GameObject)PrefabUtility.InstantiatePrefab(source);g.name=name;g.transform.position=Vector3.zero;var renderers=g.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
  float dimension=name=="character-zombie"?bounds.size.y:Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);g.transform.localScale*=height/Mathf.Max(.001f,dimension);bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);g.transform.position=position-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);g.transform.SetParent(parent,true);
  var material=Mat(pack,Color.white);material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ThirdParty/Kenney/"+pack+"/Models/Textures/colormap.png");foreach(var r in renderers)r.sharedMaterials=Enumerable.Repeat(material,r.sharedMaterials.Length).ToArray();return g;
 }
 static void BoxFor(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);var box=new GameObject(g.name+" collision");box.layer=10;box.transform.position=b.center;box.AddComponent<BoxCollider>().size=b.size;}
 static void Sign(string text,Vector3 position,float yaw=0){var g=new GameObject(text);g.transform.position=position;g.transform.rotation=Quaternion.Euler(0,yaw,0);var t=g.AddComponent<TextMeshPro>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");t.text=text;t.fontSize=3;t.alignment=TextAlignmentOptions.Center;t.rectTransform.sizeDelta=new Vector2(12,1);t.color=Accent;}
 static ParticleSystem Particles(string name,Vector3 pos,Color color,float speed,Transform parent){var g=new GameObject(name);g.transform.position=pos;g.transform.SetParent(parent,true);var p=g.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=p.main;main.playOnAwake=false;main.loop=false;main.duration=.7f;main.startLifetime=.6f;main.startSpeed=speed;main.startSize=.08f;main.startColor=color;main.maxParticles=120;var emission=p.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,100)});var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.1f;p.GetComponent<ParticleSystemRenderer>().sharedMaterial=cyan;return p;}
 static AnimatorController ZombieAnimator(){var path=Base+"Animations/Zombie.controller";var existing=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);if(existing)return existing;var c=AnimatorController.CreateAnimatorControllerAtPath(path);c.AddParameter("Speed",AnimatorControllerParameterType.Float);c.AddParameter("Attack",AnimatorControllerParameterType.Trigger);c.AddParameter("Die",AnimatorControllerParameterType.Trigger);var clips=AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/Kenney/GraveyardKit/Models/character-zombie.fbx").OfType<AnimationClip>().Where(x=>!x.name.StartsWith("__")).ToArray();Debug.Log("ZOMBIE_CLIPS "+string.Join(",",clips.Select(x=>x.name)));var sm=c.layers[0].stateMachine;AnimatorState Add(string name,string search){var s=sm.AddState(name);s.motion=clips.FirstOrDefault(x=>x.name==search)??clips.FirstOrDefault(x=>x.name.EndsWith(search));return s;}var idle=Add("Idle","idle");var walk=Add("Walk","walk");var run=Add("Run","sprint");var attack=Add("Attack","attack-melee-right");var dead=Add("Dead","die");sm.defaultState=idle;void T(AnimatorState a,AnimatorState b,AnimatorConditionMode mode,float value){var t=a.AddTransition(b);t.hasExitTime=false;t.duration=.12f;t.AddCondition(mode,value,"Speed");}T(idle,walk,AnimatorConditionMode.Greater,.1f);T(walk,idle,AnimatorConditionMode.Less,.1f);T(walk,run,AnimatorConditionMode.Greater,2.5f);T(run,walk,AnimatorConditionMode.Less,2.5f);var at=sm.AddAnyStateTransition(attack);at.hasExitTime=false;at.canTransitionToSelf=false;at.AddCondition(AnimatorConditionMode.If,0,"Attack");var dt=sm.AddAnyStateTransition(dead);dt.hasExitTime=false;dt.canTransitionToSelf=false;dt.AddCondition(AnimatorConditionMode.If,0,"Die");var back=attack.AddTransition(idle);back.hasExitTime=true;back.exitTime=.95f;return c;}
 static Canvas Canvas(){var g=new GameObject("Interface",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var c=g.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=g.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));return c;}
 static TMP_Text Text(Transform parent,string text,Vector2 position,Vector2 size,int font,Vector2? anchor=null){var g=new GameObject(text,typeof(RectTransform));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor??new Vector2(.5f,.5f);r.sizeDelta=size;r.anchoredPosition=position;var t=g.AddComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");t.fontSize=font;t.text=text;t.color=Color.white;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;return t;}
 static GameObject Panel(Transform parent,string name,Color color){var g=new GameObject(name,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;g.GetComponent<Image>().color=color;return g;}
 static Button Button(Transform parent,string text,float y){var g=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.sizeDelta=new Vector2(430,66);r.anchoredPosition=new Vector2(0,y);g.GetComponent<Image>().color=new Color(.06f,.3f,.34f);var button=g.GetComponent<Button>();var colors=button.colors;colors.highlightedColor=Accent;button.colors=colors;Text(g.transform,text,Vector2.zero,new Vector2(420,60),23);return button;}
 static GameObject Modal(Transform parent,string title,string sub,bool pause){var p=Panel(parent,title,new Color(.01f,.035f,.05f,.96f));Text(p.transform,title,new Vector2(0,200),new Vector2(1300,100),54);Text(p.transform,sub,new Vector2(0,110),new Vector2(1200,80),25);var m=p.AddComponent<MenuController>();if(pause){Set(m,"resume",Button(p.transform,"CONTINUAR",10));Set(m,"volume",Slider(p.transform,"VOLUMEN",-265,0,1));Set(m,"sensitivity",Slider(p.transform,"SENSIBILIDAD",-350,.03f,.3f));}Set(m,"retry",Button(p.transform,"REINTENTAR",-75));Set(m,"menu",Button(p.transform,"VOLVER AL MENÚ",-160));return p;}
 static Slider Slider(Transform parent,string label,float y,float min,float max)
 {
  Text(parent,label,new Vector2(-300,y),new Vector2(240,45),20);
  var g=new GameObject(label,typeof(RectTransform),typeof(Slider));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.sizeDelta=new Vector2(400,28);r.anchoredPosition=new Vector2(110,y);
  var bg=Panel(g.transform,"Track",new Color(.12f,.22f,.26f));var fill=Panel(g.transform,"Fill",Accent);var handle=Panel(g.transform,"Handle",Color.white);var hr=handle.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=new Vector2(.5f,.5f);hr.sizeDelta=new Vector2(22,40);
  var slider=g.GetComponent<Slider>();slider.fillRect=fill.GetComponent<RectTransform>();slider.handleRect=hr;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=min;slider.maxValue=max;return slider;
 }
 static void GameplayUI(){var c=Canvas();var h=c.gameObject.AddComponent<HUDController>();Set(h,"hp",Text(c.transform,"SALUD",new Vector2(220,60),new Vector2(380,60),30,new Vector2(0,0)));Set(h,"ammo",Text(c.transform,"MUNICIÓN",new Vector2(-220,60),new Vector2(380,60),30,new Vector2(1,0)));Set(h,"objective",Text(c.transform,"ESTACIONES",new Vector2(0,-65),new Vector2(1100,60),28,new Vector2(.5f,1)));Set(h,"pulse",Text(c.transform,"PULSO",new Vector2(-230,120),new Vector2(400,50),22,new Vector2(1,0)));Set(h,"prompt",Text(c.transform,"",new Vector2(0,-90),new Vector2(1000,60),24));Text(c.transform,"+",Vector2.zero,new Vector2(30,30),24);Text(c.transform,"WASD  MOVER     SHIFT  CORRER     R  RECARGAR     Q  PULSO     E  INTERACTUAR     ESC  PAUSA",new Vector2(0,22),new Vector2(1300,30),16,new Vector2(.5f,0));var pause=Modal(c.transform,"TRANSMISIÓN EN PAUSA","La muestra está a salvo. Retoma la misión cuando estés listo.",true);Set(h,"pausePanel",pause);pause.SetActive(false);var win=Modal(c.transform,"MUESTRA RECUPERADA","Tres estaciones en línea. La cura tiene una oportunidad.",false);Set(h,"winPanel",win);win.SetActive(false);var lose=Modal(c.transform,"SEÑAL PERDIDA","Los infectados encontraron al portador. Inténtalo de nuevo.",false);Set(h,"losePanel",lose);lose.SetActive(false);}
 static void MainMenu(){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var camera=new GameObject("MenuCamera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;new GameObject("GameManager").AddComponent<GameManager>();var c=Canvas();var p=Panel(c.transform,"Main menu",Ink);Text(p.transform,"LÁZARO / BIOSECURITY DIVISION",new Vector2(0,350),new Vector2(1200,60),23).color=Accent;Text(p.transform,"PROTOCOLO\nLÁZARO",new Vector2(0,170),new Vector2(1400,230),92);Text(p.transform,"LA CURA TE DELATA",new Vector2(0,15),new Vector2(1000,60),28).color=Accent;Text(p.transform,"Reinicia tres estaciones. Conserva la muestra. Regresa a la esclusa.\nTu pulso paraliza a los infectados cercanos… y atrae a los demás.",new Vector2(0,-95),new Vector2(1200,100),25);var m=p.AddComponent<MenuController>();Set(m,"start",Button(p.transform,"INICIAR MISIÓN",-220));Set(m,"quit",Button(p.transform,"SALIR",-305));Text(p.transform,"MODELOS Y SONIDOS: KENNEY · CC0   /   UNITY 6 · URP",new Vector2(0,35),new Vector2(1200,40),16,new Vector2(.5f,0));EditorSceneManager.SaveScene(scene,Base+"Scenes/MainMenu.unity");}
 public static void Validate(){foreach(var path in new[]{Base+"Scenes/MainMenu.unity",Base+"Scenes/Laboratory_Main.unity"}){var s=EditorSceneManager.OpenScene(path);foreach(var root in s.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0)throw new Exception("Missing script: "+t.name);if(!UnityEngine.Object.FindAnyObjectByType<GameManager>())throw new Exception("Missing GameManager");if(path.Contains("Laboratory")){if(UnityEngine.Object.FindObjectsByType<ZombieAI>(FindObjectsSortMode.None).Length!=4)throw new Exception("Enemy count");if(UnityEngine.Object.FindObjectsByType<PowerStation>(FindObjectsSortMode.None).Length!=3)throw new Exception("Station count");if(!UnityEngine.Object.FindAnyObjectByType<NavMeshSurface>().navMeshData)throw new Exception("Missing saved navigation");}}}
}
