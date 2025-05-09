using UnityEngine;

namespace NeuroViz.Scenes.Connected
{
    public class IdleScene : MonoBehaviour
    {
        [SerializeField] private GameObject brain;


        private void OnEnable()
        {
            brain.SetActive(false);
        }

        private void OnDisable()
        {
            brain.SetActive(true);
        }
    }
}