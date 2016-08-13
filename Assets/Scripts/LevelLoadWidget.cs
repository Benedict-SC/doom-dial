using UnityEngine;

public class LevelLoadWidget : MonoBehaviour
{
    public void LoadLevel(string levelname)
    {
        Application.LoadLevel(levelname);
    }
}