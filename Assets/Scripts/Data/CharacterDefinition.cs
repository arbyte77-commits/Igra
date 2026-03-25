using UnityEngine;

namespace BeachRunner.Data
{
    [CreateAssetMenu(menuName = "A%A/Character Definition", fileName = "CharacterDefinition")]
    public class CharacterDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite portrait;
        public RuntimeAnimatorController animatorController;
    }
}
