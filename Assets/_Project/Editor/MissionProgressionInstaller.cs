using System;
using System.Linq;
using ProtocoloLazaro;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
public static class MissionProgressionInstaller
{
    [MenuItem("Lazaro/Update Mission Progression")]
    public static void Install()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Laboratory_Main.unity");
        var previous = GameObject.Find("Mission progression");
        if (previous) UnityEngine.Object.DestroyImmediate(previous);
        var root = new GameObject("Mission progression");
        var stations = UnityEngine.Object.FindObjectsByType<PowerStation>(FindObjectsSortMode.None).OrderBy(s => s.name).ToArray();
        var enemies = UnityEngine.Object.FindObjectsByType<ZombieAI>(FindObjectsSortMode.None);
        var mission = UnityEngine.Object.FindFirstObjectByType<MissionManager>();
        Vector3[] fuses = { new(-5,1.8f,5), new(5,.9f,5), new(6,.9f,-5) };
        Vector3[] first = { new(-16,1,6), new(6,1,10), new(6,1,-10) };
        Vector3[] second = { new(-7,1,10), new(16,1,6), new(16,1,-6) };
        Vector3[] reservePositions = { new(-17,0,-11), new(17,0,-11), new(17,0,11) };
        var reserves = new GameObject[3];
        for (int i=0;i<stations.Length;i++)
        {
            string sector = ((char)('A'+i)).ToString();
            var config = new SerializedObject(stations[i]);
            config.FindProperty("sector").stringValue=sector;
            config.FindProperty("calibrationDuration").floatValue=55;
            config.ApplyModifiedPropertiesWithoutUndo();
            Item(root.transform,"FUSIBLE "+sector,fuses[i],new Vector3(.55f,.3f,.35f),"Medical",MissionItemKind.Fuse,stations[i]);
            Item(root.transform,"PANEL "+sector+"-1",first[i],new Vector3(.9f,.6f,.25f),"Cyan",MissionItemKind.AdjustmentOne,stations[i]);
            Item(root.transform,"PANEL "+sector+"-2",second[i],new Vector3(.9f,.6f,.25f),"Cyan",MissionItemKind.AdjustmentTwo,stations[i]);
            reserves[i]=UnityEngine.Object.Instantiate(enemies[i % enemies.Length].gameObject,root.transform);
            reserves[i].name="Reserve infected "+sector;
            reserves[i].GetComponent<NavMeshAgent>().enabled=false;
            reserves[i].transform.position=reservePositions[i];
            reserves[i].SetActive(false);
        }
        Item(root.transform,"MUESTRA · CONTENCIÓN",new Vector3(15,1,-12),new Vector3(.45f,.8f,.45f),"Medical",MissionItemKind.Sample,null);
        var serializedMission=new SerializedObject(mission);
        var array=serializedMission.FindProperty("reinforcements");array.arraySize=3;
        for(int i=0;i<3;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=reserves[i];
        serializedMission.ApplyModifiedPropertiesWithoutUndo();
        var medkit=UnityEngine.Object.FindFirstObjectByType<Medkit>();
        foreach(var position in new[]{new Vector3(-6,.6f,12),new Vector3(17,.6f,10)})
        {var copy=UnityEngine.Object.Instantiate(medkit.gameObject,root.transform);copy.name="Emergency medkit";copy.transform.position=position;}
        EditorSceneManager.SaveScene(scene);
        MenuCinematicInstaller.Install();
        var menu=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach(var label in UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
            if(label.text.StartsWith("Reinicia tres estaciones."))label.text="Recoge fusibles. Instálalos y calibra las tres estaciones.\nAjusta los paneles, recupera la muestra y vuelve a la esclusa.";
        EditorSceneManager.SaveScene(menu);
        AssetDatabase.SaveAssets();
        Debug.Log("LAZARO_PROGRESSION_SAVED");
    }
    private static void Item(Transform root,string name,Vector3 position,Vector3 scale,string material,MissionItemKind kind,PowerStation station)
    {
        var item=GameObject.CreatePrimitive(PrimitiveType.Cube);item.name=name;item.transform.SetParent(root);item.transform.position=position;item.transform.localScale=scale;
        item.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/"+material+".mat");
        if(kind==MissionItemKind.AdjustmentOne || kind==MissionItemKind.AdjustmentTwo)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ThirdParty/Kenney/SpaceStationKit/Models/computer-screen.fbx");
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(source);
            visual.name="Calibration terminal";
            visual.transform.rotation=Quaternion.Euler(0,180,0);
            var renderers=visual.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            visual.transform.localScale*=.9f/Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
            bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            visual.transform.position+=position-bounds.center;
            visual.transform.SetParent(item.transform,true);
            var palette=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/SpaceStationKit.mat");
            foreach(var renderer in renderers)renderer.sharedMaterials=Enumerable.Repeat(palette,renderer.sharedMaterials.Length).ToArray();
            item.GetComponent<Renderer>().enabled=false;
            var stand=GameObject.CreatePrimitive(PrimitiveType.Cube);stand.name="Terminal stand";UnityEngine.Object.DestroyImmediate(stand.GetComponent<Collider>());stand.transform.position=new Vector3(position.x,.4f,position.z);stand.transform.localScale=new Vector3(.18f,.8f,.18f);stand.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Trim.mat");stand.transform.SetParent(item.transform,true);
        }
        var behaviour=item.AddComponent<MissionItem>();var data=new SerializedObject(behaviour);data.FindProperty("kind").enumValueIndex=(int)kind;data.FindProperty("station").objectReferenceValue=station;data.ApplyModifiedPropertiesWithoutUndo();
        var label=new GameObject(name+" label").AddComponent<TextMeshPro>();label.transform.SetParent(root);label.transform.position=position+Vector3.up*.65f;label.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");label.text=name;label.fontSize=.65f;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(5,1);label.color=Color.cyan;
    }
}
