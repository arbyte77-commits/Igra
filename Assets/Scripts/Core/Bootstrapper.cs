using System.Collections;
using UnityEngine;

namespace BeachRunner.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private float splashDuration = 1.5f;

        private IEnumerator Start()
        {
            GameManager.Instance.ChangeState(GameState.Splash);
            yield return new WaitForSeconds(splashDuration);
            GameManager.Instance.ChangeState(GameState.MainMenu);
        }
    }
}
