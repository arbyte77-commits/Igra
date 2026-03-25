using UnityEngine;
using BeachRunner.Core;

namespace BeachRunner.Player
{
    public class CharacterSpawner : MonoBehaviour
    {
        [SerializeField] private RunnerController runnerPrefab;

        private void Start()
        {
            var runner = Instantiate(runnerPrefab, transform.position, Quaternion.identity);
            var controller = GameManager.Instance.SelectedCharacter.animatorController;
            if (controller != null)
            {
                runner.GetComponent<Animator>().runtimeAnimatorController = controller;
            }
        }
    }
}
