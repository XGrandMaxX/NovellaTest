using UnityEngine;

public class MapNavigationInput : MonoBehaviour
{
    [SerializeField] private MapTransition mapTransition;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            mapTransition.MoveLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            mapTransition.MoveRight();
        }
    }
}