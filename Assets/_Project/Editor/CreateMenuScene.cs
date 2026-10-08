using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProtocoloLazaro;

public static class CreateMenuScene
{
    [MenuItem("Lazaro/Create Main Menu")]
    public static void Create()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var gm=new GameObject("GameManager");gm.AddComponent<GameManager>();
        var canvas=new GameObject("MainMenuCanvas");var c=canvas.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;canvas.AddComponent<CanvasScaler>();canvas.AddComponent<GraphicRaycaster>();
        var title=Text(canvas.transform,"PROTOCOLO LÁZARO",46,new Vector2(0,130));
        var sub=Text(canvas.transform,"La cura te delata",24,new Vector2(0,70));
        var b=new GameObject("StartButton");b.transform.SetParent(canvas.transform,false);var br=b.AddComponent<RectTransform>();br.sizeDelta=new Vector2(260,70);br.anchoredPosition=new Vector2(0,-20);var img=b.AddComponent<Image>();img.color=new Color(.05f,.35f,.42f,.95f);var btn=b.AddComponent<Button>();btn.onClick.AddListener(()=>SceneManager.LoadScene("Laboratory_Main"));var label=Text(b.transform,"INICIAR MISIÓN",22,Vector2.zero);label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=Vector2.zero;label.rectTransform.offsetMax=Vector2.zero;
        Text(canvas.transform,"WASD mover  |  Mouse mirar  |  Click disparar  |  Q pulso  |  E interactuar  |  Esc pausa",16,new Vector2(0,-130));
        EditorSceneManager.SaveScene(scene,"Assets/_Project/Scenes/MainMenu.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/_Project/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene("Assets/_Project/Scenes/Laboratory_Main.unity",true)};
        AssetDatabase.SaveAssets();
    }
    private static Text Text(Transform p,string value,int size,Vector2 pos){var go=new GameObject(value);go.transform.SetParent(p,false);var r=go.AddComponent<RectTransform>();r.sizeDelta=new Vector2(1100,70);r.anchoredPosition=pos;var t=go.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;return t;}
}
