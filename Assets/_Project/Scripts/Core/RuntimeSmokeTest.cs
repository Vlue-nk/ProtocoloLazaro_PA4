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
            bool requested = Environment.GetCommandLineArgs().Contains("--lazaro-test");
            #if UNITY_EDITOR
            requested |= UnityEditor.SessionState.GetBool("Lazaro.RunValidation", false);
            UnityEditor.SessionState.SetBool("Lazaro.RunValidation", false);
            #endif
            if (!requested) return;
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
                var enemyAudio = enemy.GetComponent<AudioSource>();
                Check(enemyAudio && enemyAudio.spatialBlend == 1 && enemyAudio.maxDistance == 14, "enemy audio is positional");
                enemy.enabled = false;
                enemy.StopAllCoroutines();
                enemy.GetComponent<NavMeshAgent>().isStopped = true;
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
            var origin = player.transform.position;
            var visibleEnemy = enemies[0];
            var blockedEnemy = enemies[1];
            Check(visibleEnemy.GetComponent<NavMeshAgent>().Warp(origin + Vector3.left * 2), "visible pulse receiver positioned");
            Check(blockedEnemy.GetComponent<NavMeshAgent>().Warp(origin + Vector3.right * 2), "occluded pulse receiver positioned");
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Temporary validation occluder";
            blocker.layer = 10;
            blocker.transform.position = origin + Vector3.right + Vector3.up;
            blocker.transform.localScale = new Vector3(.2f, 3, 2);
            Physics.SyncTransforms();
            Check(pulse.TryPulse(), "pulse during reload");
            Check(visibleEnemy.State == ZombieState.Stunned, "pulse stuns visible nearby enemy");
            Check(blockedEnemy.State == ZombieState.Investigate, "wall blocks stun but transmits noise");
            visibleEnemy.Damage(35);
            visibleEnemy.Damage(35);
            Check(!visibleEnemy.IsDead, "two hits do not kill infected");
            visibleEnemy.Damage(35);
            Check(visibleEnemy.IsDead && !visibleEnemy.GetComponent<Collider>().enabled, "third hit kills and removes collision");
            visibleEnemy.Stun(3);
            visibleEnemy.Hear(origin);
            Check(visibleEnemy.IsDead, "dead enemy ignores pulse and noise");
            Destroy(blocker);
            var attacker = Instantiate(enemies[2], origin + Vector3.forward, Quaternion.Euler(0, 180, 0));
            attacker.name = "Temporary validation attacker";
            Check(attacker.GetComponent<NavMeshAgent>().Warp(origin + Vector3.forward), "attacker positioned on NavMesh");
            attacker.enabled = true;
            yield return null;
            yield return null;
            Check(attacker.State == ZombieState.Attack, "nearby visible player triggers attack windup");
            attacker.Stun(3);
            yield return new WaitForSeconds(.5f);
            Check(player.Current == 80 && attacker.State == ZombieState.Stunned, "stun cancels pending melee damage");
            Destroy(attacker.gameObject);
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
            GameManager.Instance.Restart();
            yield return null;
            yield return null;
            player = FindAnyObjectByType<PlayerHealth>();
            player.GetComponent<PlayerController>().enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            foreach (var enemy in FindObjectsByType<ZombieAI>(FindObjectsSortMode.None))
            {
                enemy.enabled = false;
                enemy.StopAllCoroutines();
                enemy.GetComponent<NavMeshAgent>().isStopped = true;
            }
            var aimedStation = FindAnyObjectByType<PowerStation>();
            player.transform.position = aimedStation.transform.position + Vector3.back * 2;
            var aimingCamera = player.GetComponentInChildren<Camera>();
            aimingCamera.transform.LookAt(aimedStation.transform.position + Vector3.up * .85f);
            Physics.SyncTransforms();
            Check(player.GetComponent<PulseEmitter>().TryPulse(), "aimed station pulse emitted");
            Check(aimedStation.IsActive && FindAnyObjectByType<MissionManager>().Activated == 1, "camera raycast activates aimed station");
            GameManager.Instance.Menu();
            yield return null;
            yield return null;
            Check(GameManager.Instance.State == GameState.MainMenu && Time.timeScale == 1 && !AudioListener.pause, "menu restores global state");
            Debug.Log("LAZARO_TEST_COMPLETE");
            #if UNITY_EDITOR
            if (Environment.GetCommandLineArgs().Contains("--lazaro-test")) UnityEditor.EditorApplication.Exit(0);
            else UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit(0);
            #endif
        }
    }
}
#endif
