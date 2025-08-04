using UnityEngine;

public class FramerateManager : MonoBehaviour
{
     void Awake()
    {
        // Le pedimos al juego que intente correr a 60 FPS.
        Application.targetFrameRate = 60;
    }
}
