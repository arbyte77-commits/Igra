using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BeachRunner.Core;
using BeachRunner.Data;

namespace BeachRunner.UI
{
    public class CharacterSelectPresenter : MonoBehaviour
    {
        [SerializeField] private CharacterDatabase database;
        [SerializeField] private Image previewImage;
        [SerializeField] private TMP_Text nameLabel;

        private int _index;

        private void OnEnable()
        {
            _index = 0;
            Refresh();
        }

        public void Next()
        {
            _index = (_index + 1) % database.Characters.Count;
            Refresh();
        }

        public void Previous()
        {
            _index = (_index - 1 + database.Characters.Count) % database.Characters.Count;
            Refresh();
        }

        public void SelectCurrent()
        {
            GameManager.Instance.SetCharacter(database.Characters[_index].id);
        }

        private void Refresh()
        {
            var selected = database.Characters[_index];
            previewImage.sprite = selected.portrait;
            nameLabel.text = selected.displayName;
            GameManager.Instance.SetCharacter(selected.id);
        }
    }
}
