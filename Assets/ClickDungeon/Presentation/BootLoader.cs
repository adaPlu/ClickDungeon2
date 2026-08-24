using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClickDungeon.Presentation
{
    public sealed class BootLoader : MonoBehaviour
    {
        private void Start() => SceneManager.LoadScene("Main");
    }
}
