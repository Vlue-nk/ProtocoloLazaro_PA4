using UnityEngine;

namespace ProtocoloLazaro
{
    [RequireComponent(typeof(Camera))]
    public sealed class MenuCinematic : MonoBehaviour
    {
        [SerializeField] private Vector3 center = new Vector3(-17, 2.7f, 4);
        [SerializeField] private Vector3 focus = new Vector3(-10, 1.5f, 8);
        [SerializeField] private float duration = 32;
        [SerializeField] private Vector3 travel = new Vector3(1.1f, .25f, 1.3f);
        private float elapsed;

        private void LateUpdate()
        {
            elapsed += Time.unscaledDeltaTime;
            float phase = elapsed * Mathf.PI * 2 / Mathf.Max(1, duration);
            transform.position = center + new Vector3(
                Mathf.Sin(phase) * travel.x,
                Mathf.Sin(phase) * travel.y,
                Mathf.Cos(phase) * travel.z);
            transform.LookAt(focus);
        }
    }
}
