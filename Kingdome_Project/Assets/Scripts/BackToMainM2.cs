using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainM2 : MonoBehaviour
{
    public void BackToMain()
    {
        SceneManager.LoadScene("MainMenu");
    }
}