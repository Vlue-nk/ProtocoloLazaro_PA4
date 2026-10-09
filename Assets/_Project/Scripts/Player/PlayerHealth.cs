using System;
using UnityEngine;
namespace ProtocoloLazaro
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth=100;
        public int Current
        {
            get;
            private set;
        }
        public int Maximum=>maxHealth;
        public event Action<int,int> HealthChanged;
        public event Action Died;
        public event Action Hurt;
        void Awake()
        {
            Current=maxHealth;
        }
        public void Damage(int amount)
        {
            if(!GameManager.IsPlaying||Current<=0||amount<=0)return;
            Current=Mathf.Max(0,Current-amount);
            HealthChanged?.Invoke(Current,maxHealth);
            Hurt?.Invoke();
            if(Current==0)
            {
                Died?.Invoke();
                GameManager.Instance.Lose();
            }
        }
        public void Heal(int amount)
        {
            if(!GameManager.IsPlaying||Current<=0)return;
            Current=Mathf.Min(maxHealth,Current+Mathf.Max(0,amount));
            HealthChanged?.Invoke(Current,maxHealth);
        }
    }
}
