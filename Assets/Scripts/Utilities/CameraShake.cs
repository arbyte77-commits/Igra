using System.Collections;
using UnityEngine;

namespace BeachRunner.World
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 _origin;

        private void Awake()
        {
            Instance = this;
            _origin = transform.localPosition;
        }

        public void Shake(float duration, float strength)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.localPosition = _origin + (Vector3)(Random.insideUnitCircle * strength);
                yield return null;
            }

            transform.localPosition = _origin;
        }
    }
}
