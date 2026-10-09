#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
namespace ProtocoloLazaro
{
    public sealed class RuntimeSmokeTest : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]         private static void Begin()
        {
            if (!Environment.GetCommandLineArgs().Contains("--lazaro-test")) return;
            var runner = new GameObject("Runtime validation");
            DontDestroyOnLoad(runner);
            runner.AddComponent<RuntimeSmokeTest>();
        }
        private static void Check(bool condition, string name)
        {
            if (!condition) throw new Exception("LAZARO_TEST_FAIL " + name);
            Debug.Log("LAZARO_TEST_PASS " + name);
        }
        private IEnumerator Start()
        {
            yield return null;
            Check(GameManager.Instance.State == GameState.MainMenu, "initial menu state");
            GameManager.Instance.Restart();
            yield return null;
            yield return null;
            var player = FindAnyObjectByType<PlayerHealth>();
            var weapon = FindAnyObjectByType<WeaponRaycast>();
            var pulse = FindAnyObjectByType<PulseEmitter>();
            var enemies = FindObjectsByType<ZombieAI>(FindObjectsSortMode.None);
            Check(enemies.Length == 4, "four enemies restored");
            foreach (var enemy in enemies)
            {
                Check(enemy.GetComponent<NavMeshAgent>().isOnNavMesh, "enemy on NavMesh");
                enemy.enabled = false;
                enemy.StopAllCoroutines();
            }
            Check(player.Current == 100 && weapon.Magazine == 6 && weapon.Reserve == 6, "initial resources");
            var medkit = FindAnyObjectByType<Medkit>();
            medkit.Use(player);
            Check(medkit.gameObject.activeSelf, "medkit retained at full health");
            player.Damage(50);
            medkit.Use(player);
            Check(player.Current == 80 && !medkit.gameObject.activeSelf, "medkit heals once");
            Check(weapon.TryShoot(), "shoot accepted");
            Check(weapon.Magazine == 5 && weapon.Reserve == 6, "shot consumes one round");
            Check(weapon.TryReload() && !weapon.TryReload(), "reload cannot duplicate");
            Check(pulse.TryPulse(), "pulse during reload");
            Check(!pulse.TryPulse(), "pulse cooldown enforced");
            float remaining = pulse.Remaining;
            GameManager.Instance.Pause();
            yield return new WaitForSecondsRealtime(.3f);
            player.Damage(25);
            Check(player.Current == 80 && weapon.Magazine == 5 && Mathf.Abs(pulse.Remaining - remaining) < .02f, "pause freezes resources and timers");
            GameManager.Instance.Resume();
            yield return new WaitForSeconds(1.6f);
            Check(weapon.Magazine == 6 && weapon.Reserve == 5, "reload conserves ammunition");
            var mission = FindAnyObjectByType<MissionManager>();
            var stations = FindObjectsByType<PowerStation>(FindObjectsSortMode.None);
            var door = FindAnyObjectByType<ExtractionDoor>();
            door.TryOpen(GameManager.Instance);
            Check(GameManager.IsPlaying, "exit locked before objectives");
            for (int i = stations.Length - 1; i >= 0; i--)
            {
                stations[i].Activate(mission);
                stations[i].Activate(mission);
            }
            Check(mission.Activated == 3 && door.IsUnlocked, "unique station activation and unlock");
            door.TryOpen(GameManager.Instance);
            Check(GameManager.Instance.State == GameState.Won, "victory");
            player.Damage(500);
            Check(player.Current == 80, "damage blocked after result");
            for(int i=0;i<3;i++)
            {
                GameManager.Instance.Restart();
                yield return null;
                yield return null;
                player = FindAnyObjectByType<PlayerHealth>();
                Check(player.Current == 100 && FindAnyObjectByType<MissionManager>().Activated == 0 && Time.timeScale == 1, "restart " + (i+1));
                player.Damage(100);
                Check(GameManager.Instance.State == GameState.Lost, "defeat " + (i+1));
            }
            GameManager.Instance.Menu();
            yield return null;
            yield return null;
            Check(GameManager.Instance.State == GameState.MainMenu && Time.timeScale == 1 && !AudioListener.pause, "menu restores global state");
            Debug.Log("LAZARO_TEST_COMPLETE");
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(0);
            #else
            Application.Quit(0);
            #endif
        }
    }
}
#endif
