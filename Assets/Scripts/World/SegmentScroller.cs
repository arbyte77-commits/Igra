using UnityEngine;
using BeachRunner.Core;

namespace BeachRunner.World
{
    public class SegmentScroller : MonoBehaviour
    {
        [SerializeField] private float recycleX = -26f;
        [SerializeField] private float forwardSpawnX = 52f;

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Playing) return;

            transform.Translate(Vector3.left * (RunnerSpeedRuntime.CurrentSpeed * Time.deltaTime), Space.World);
            if (transform.position.x < recycleX)
            {
                transform.position += new Vector3(forwardSpawnX, 0f, 0f);
            }
        }
    }
}
