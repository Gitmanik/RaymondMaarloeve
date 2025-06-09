using UnityEngine;
using UnityEngine.SceneManagement;

public class RunGameScript : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
