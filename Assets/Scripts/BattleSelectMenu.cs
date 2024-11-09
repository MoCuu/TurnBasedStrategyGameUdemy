using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSelectMenu : MonoBehaviour
{
    public string mainMenu;

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenu);
    }

    public void LoadMission(string missionToLoad)
    {
        SceneManager.LoadScene(missionToLoad);
    }
}
