using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace ProtocoloLazaro
{
    public sealed class Medkit : MonoBehaviour
    {
        [SerializeField] private int amount=30;
        private bool used;
        public void Use(PlayerHealth target)
        {
            if(used||!target||target.Current>=100)return;
            used=true;
            target.Heal(amount);
            gameObject.SetActive(false);
        }
    }
}
