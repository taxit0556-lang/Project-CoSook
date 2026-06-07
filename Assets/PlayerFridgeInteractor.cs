using UnityEngine;

public class PlayerFridgeInteraction : MonoBehaviour
{
    public Transform fridge;
    public float interactDistance = 3f;

    void Update()
    {
        float distance = Vector3.Distance(transform.position, fridge.position);

        if (distance <= interactDistance)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                FridgeController.Instance.OpenFridge();
            }
        }
    }
}