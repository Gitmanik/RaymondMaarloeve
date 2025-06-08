using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void LoadSettingsScene()
    {
        SceneManager.LoadScene("Settings");
    }
}
