using UnityEngine;
using TMPro;
namespace ProtocoloLazaro
{
    public sealed class HUDController : MonoBehaviour
    {
        [SerializeField] private TMP_Text hp,ammo,objective,pulse,prompt;
        [SerializeField] private GameObject pausePanel,winPanel,losePanel;
        private PlayerHealth health;
        private WeaponRaycast weapon;
        private MissionManager mission;
        private PulseEmitter emitter;
        private PlayerController player;
        private GameManager game;
        void OnEnable()
        {
            health=FindFirstObjectByType<PlayerHealth>();
            weapon=FindFirstObjectByType<WeaponRaycast>();
            mission=FindFirstObjectByType<MissionManager>();
            emitter=FindFirstObjectByType<PulseEmitter>();
            player=FindFirstObjectByType<PlayerController>();
            game=FindFirstObjectByType<GameManager>();
            if(health)health.HealthChanged+=SetHealth;
            if(weapon)weapon.AmmoChanged+=SetAmmo;
            if(mission)mission.ProgressChanged+=SetMission;
            if(game)game.StateChanged+=ShowState;
        }
        void Start()
        {
            if(health)SetHealth(health.Current,health.Maximum);
            if(weapon)SetAmmo(weapon.Magazine,weapon.Reserve);
            if(mission)SetMission(mission.Activated,3);
            if(game)ShowState(game.State);
        }
        void OnDisable()
        {
            if(health)health.HealthChanged-=SetHealth;
            if(weapon)weapon.AmmoChanged-=SetAmmo;
            if(mission)mission.ProgressChanged-=SetMission;
            if(game)game.StateChanged-=ShowState;
        }
        void Update()
        {
            if(pulse&&emitter)pulse.text=emitter.Remaining<=0?"[Q] PULSO LISTO":"PULSO Â· "+Mathf.CeilToInt(emitter.Remaining)+" s";
            if(prompt&&player)prompt.text=player.Prompt;
            if(ammo&&weapon&&weapon.IsReloading)ammo.text="RECARGANDOâ€¦";
            else if(ammo&&weapon)SetAmmo(weapon.Magazine,weapon.Reserve);
        }
        void SetHealth(int a,int b)
        {
            if(hp)hp.text=$"SALUD   {a} / {b}";
        }
        void SetAmmo(int a,int b)
        {
            if(ammo)ammo.text=$"MUNICIÃ“N   {a} / {b}";
        }
        void SetMission(int a,int b)
        {
            if(objective)objective.text=a==b?"MUESTRA ESTABLE Â· REGRESA A EXTRACCIÃ“N":$"REINICIA LAS ESTACIONES   {a} / {b}";
        }
        void ShowState(GameState state)
        {
            if(pausePanel)pausePanel.SetActive(state==GameState.Paused);
            if(winPanel)winPanel.SetActive(state==GameState.Won);
            if(losePanel)losePanel.SetActive(state==GameState.Lost);
        }
    }
}
