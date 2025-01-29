using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerAnimation playerAnimation;
    [SerializeField] PlayerAudio playerAudio;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] PlayerPhysics playerPhysics;
    [SerializeField] PlayerHealthCondition playerHealthCondition;
    [SerializeField] PlayerCamMechanicManager playerCamMechanicManager;
    [SerializeField] PlayerCamMechanicCore playerCamMechanicCore;

    public PlayerAnimation PlayerAnimation => playerAnimation;
    public PlayerAudio PlayerAudio => playerAudio;
    public PlayerController PlayerController => playerController;
    public PlayerMovement PlayerMovement => playerMovement;
    public PlayerPhysics PlayerPhysics => playerPhysics;
    public PlayerHealthCondition PlayerHealthCondition => playerHealthCondition;
    public PlayerCamMechanicManager PlayerCamMechanicManager => playerCamMechanicManager;
    public PlayerCamMechanicCore PlayerCamMechanicCore => playerCamMechanicCore;

    private void Awake()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}