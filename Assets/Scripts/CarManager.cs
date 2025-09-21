using UnityEngine;

public class CarManager : MonoBehaviour
{
    [Header("References")] public FPSController player;
    public CarController currentCar;
    
    private Vector3 playerExitPosition;


    void Update()
    {
        if (currentCar.playerInCar && Input.GetKeyDown(KeyCode.F))
        {
            ExitCar();
        }
    }

    public void EnterCar(FPSController playerController, CarController car)
    {
        currentCar = car;

        car.playerInCar = true;
        playerController.gameObject.SetActive(false);

    }

    public void ExitCar()
    {

        currentCar.playerInCar = false;

        player.transform.position = currentCar.transform.position + Vector3.left * 2f + Vector3.up;
        player.gameObject.SetActive(true);
        
        currentCar = null;

    }

}