using UnityEngine;
using UnityEngine.UI;
namespace ProtocoloLazaro
{
    public sealed class MenuController : MonoBehaviour
    {
        [SerializeField] private Button start,resume,retry,menu,quit;
        [SerializeField] private Slider volume,sensitivity;
        private GameManager game;
        void Start()
        {
            game=FindFirstObjectByType<GameManager>();
            if(start)start.onClick.AddListener(game.Restart);
            if(resume)resume.onClick.AddListener(game.Resume);
            if(retry)retry.onClick.AddListener(game.Restart);
            if(menu)menu.onClick.AddListener(game.Menu);
            if(quit)quit.onClick.AddListener(game.Quit);
            if(volume)
            {
                volume.value=AudioListener.volume;
                volume.onValueChanged.AddListener(SetVolume);
            }
            if(sensitivity)
            {
                var p=FindFirstObjectByType<PlayerController>();
                if(p)
                {
                    sensitivity.value=p.Sensitivity;
                    sensitivity.onValueChanged.AddListener(v=>p.Sensitivity=v);
                }
            }
        }
        void SetVolume(float v)
        {
            AudioListener.volume=v;
        }
    }
}
