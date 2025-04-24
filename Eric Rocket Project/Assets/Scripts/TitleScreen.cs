using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame()
    {
        // Load the scene with the name "Game"
        SceneManager.LoadScene("Combined");
    }

    public void quit()
    {
        Application.Quit();
    }
}
