using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeachRunner.Data
{
    [CreateAssetMenu(menuName = "A%A/Character Database", fileName = "CharacterDatabase")]
    public class CharacterDatabase : ScriptableObject
    {
        [SerializeField] private List<CharacterDefinition> characters = new();
        public List<CharacterDefinition> Characters => characters;
        public CharacterDefinition DefaultCharacter => characters.Count > 0 ? characters[0] : null;

        public CharacterDefinition GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return DefaultCharacter;
            return characters.FirstOrDefault(c => c.id == id) ?? DefaultCharacter;
        }
    }
}
