using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    // Start is called before the first frame update
    public void LevelSelection()
    {
        SceneManager.LoadScene("level selection");
    }
    public void Level(string level)
    {
        SceneManager.LoadScene(level);
    }

    // Update is called once per frame
    public void Quit()
    {
        Application.Quit();
    }
}
