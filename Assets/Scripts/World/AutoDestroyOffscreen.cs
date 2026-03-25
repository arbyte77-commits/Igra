using UnityEngine;

namespace BeachRunner.World
{
    public class AutoDestroyOffscreen : MonoBehaviour
    {
        [SerializeField] private float minX = -30f;

        private void Update()
        {
            if (transform.position.x < minX)
            {
                Destroy(gameObject);
            }
        }
    }
}
