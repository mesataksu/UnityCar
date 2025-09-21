using UnityEngine;

public class CarManager : MonoBehaviour
{
    [Header("References")] public FPSController player;
    public CarController currentCar;

    private bool playerInCar = false;
    private Vector3 playerExitPosition;


    void Update()
    {
        if (playerInCar && Input.GetKeyDown(KeyCode.F))
        {
            ExitCar();
        }
    }

    public void EnterCar(FPSController playerController, CarController car)
    {
        currentCar = car;
        playerInCar = true;

        car.PlayerEnterCar();
        playerController.gameObject.SetActive(false);

    }

    public void ExitCar()
    {

        currentCar.PlayerExitCar();

        player.transform.position = currentCar.transform.position + Vector3.left * 2f + Vector3.up;

        player.gameObject.SetActive(true);

        playerInCar = false;
        currentCar = null;

    }

}