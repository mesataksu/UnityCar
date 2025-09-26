using UnityEngine;

public class CarManager : MonoBehaviour
{
    [Header("References")] public PlayerController player;
    public CarController currentCar;
    
    private Vector3 playerExitPosition;


    void Update()
    {
        if (currentCar.playerInCar && Input.GetKeyDown(KeyCode.F))
        {
            ExitCar();
        }
    }

    public void EnterCar(PlayerController playerController, CarController car)
    {
        currentCar = car;

        car.playerInCar = true;
        playerController.gameObject.SetActive(false);

    }

    public void ExitCar()
    {

        currentCar.playerInCar = false;

        player.transform.position = currentCar.transform.position + currentCar.transform.forward * 3f;
        player.gameObject.SetActive(true);
        
        currentCar = null;

    }

}