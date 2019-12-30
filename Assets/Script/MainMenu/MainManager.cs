using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    public Button StartButton;
    public Button ExitButton;

    void Start()
    {
        StartButton.onClick.AddListener(ChangeGameScene);
        ExitButton.onClick.AddListener(Quit);
    }

    void ChangeGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    void Quit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

        #else
        Application.Quit();

        #endif
    }
}
