using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace ProtocoloLazaro
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance
        {
            get;
            private set;
        }
        public GameState State
        {
            get;
            private set;
        }
        public static bool IsPlaying => Instance && Instance.State==GameState.Playing;
        public event Action<GameState> StateChanged;
        void Awake()
        {
            if(Instance&&Instance!=this)
            {
                Destroy(gameObject);
                return;
            }
            Instance=this;
            SetState(SceneManager.GetActiveScene().name=="MainMenu"?GameState.MainMenu:GameState.Playing);
            Application.targetFrameRate=60;
        }
        void Update()
        {
            if(Keyboard.current?.escapeKey.wasPressedThisFrame!=true)return;
            if(State==GameState.Playing)Pause();
            else if(State==GameState.Paused)Resume();
        }
        void SetState(GameState next)
        {
            State=next;
            bool play=next==GameState.Playing;
            Time.timeScale=play||next==GameState.MainMenu?1:0;
            AudioListener.pause=next==GameState.Paused||next==GameState.Won||next==GameState.Lost;
            Cursor.lockState=play?CursorLockMode.Locked:CursorLockMode.None;
            Cursor.visible=!play;
            StateChanged?.Invoke(next);
        }
        public void Pause()
        {
            if(IsPlaying)SetState(GameState.Paused);
        }
        public void Resume()
        {
            if(State==GameState.Paused)SetState(GameState.Playing);
        }
        public void Win()
        {
            if(IsPlaying)SetState(GameState.Won);
        }
        public void Lose()
        {
            if(IsPlaying)SetState(GameState.Lost);
        }
        public void Restart()
        {
            Restore();
            SceneManager.LoadScene("Laboratory_Main");
        }
        public void Menu()
        {
            Restore();
            SceneManager.LoadScene("MainMenu");
        }
        public void Quit()
        {
            Restore();
            Application.Quit();
        }
        static void Restore()
        {
            Time.timeScale=1;
            AudioListener.pause=false;
        }
        void OnDestroy()
        {
            if(Instance==this)
            {
                Instance=null;
                Restore();
            }
        }
    }
}
