using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public MovementTech playerMovement;
    public Image dashReticle;
    public Color32 readyColor = Color.cyan;
    public Color32 cooldownColor = Color.red;


    private void Start()
    {
        cooldownColor.a = 80;
        readyColor.a = 60;
    }

    void Update()
    {
        if (playerMovement == null || dashReticle == null) return;

        if (playerMovement.canDash)
        {
            readyColor.a = 60;
            dashReticle.fillAmount = 1f;
            dashReticle.color = readyColor;
        }
        else
        {
            dashReticle.color = cooldownColor;
            
            // Fills the circle up as the timer counts down to 0
            dashReticle.fillAmount = 1f - (playerMovement.dashCooldownTimer / playerMovement.dashCooldown);
        }
    }
}