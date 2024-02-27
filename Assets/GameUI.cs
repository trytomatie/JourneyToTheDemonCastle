using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameUI : MonoBehaviour
{
    [Header("Inventory")]
    public Animator inventoryAnimator;

    [Header("Building Menu")]
    public GameObject buildingMenu;
    private bool isInBuildingMode = false;

    [Header("Crafting Menu")]
    public CrafterUI crafterUI;

    [Header("Hotbar")]
    public InventoryUI inventoryUI;

    [Header("Talent Menu")]
    public GameObject talentMenuEnviorment;

    [HideInInspector] public Animator interfaceAnimator;

    // Singleton
    public static GameUI instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        interfaceAnimator = GetComponent<Animator>();
        SetUpInputSystem();
    }

    private void OnDisable()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed -= ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed -= ToggleBuildingMenu;
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed -= ctx => CloseAllWindows();
        InputSystem.GetInputActionMapPlayer().IngameUI.TalentMenu.performed -= ctx => ToggleTalentMenu();
    }

    /// <summary>
    /// Set up the input system for the inventory
    /// </summary>
    private void SetUpInputSystem()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed += ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed += ToggleBuildingMenu;
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed += ctx => CloseAllWindows();  
        InputSystem.GetInputActionMapPlayer().IngameUI.TalentMenu.performed += ctx => ToggleTalentMenu();
    }


    private void ToggleBuildingMenu(InputAction.CallbackContext ctx)
    {
        ToggleBuildingMenu();
    }

    public void ToggleBuildingMenu()
    {
        if (interfaceAnimator.GetInteger("State") != 1)
        {
            interfaceAnimator.SetInteger("State", 1);
        }
        else
        {
            interfaceAnimator.SetInteger("State", 0);
            BuildingManager.instance.PlaceBuildingMode = false;
        }
    }

    public void ToggleTalentMenu()
    {
        if (interfaceAnimator.GetInteger("State") != 3)
        {
            talentMenuEnviorment.SetActive(true);
            interfaceAnimator.SetInteger("State", 3);
        }
        else
        {
            talentMenuEnviorment.SetActive(false);
            interfaceAnimator.SetInteger("State", 0);
        }
    }

    public void SetInterfaceState(int state)
    {
        interfaceAnimator.SetInteger("State", state);
    }

    /// <summary>
    /// Toggle the inventory on and off
    /// </summary>
    /// <param name="ctx"></param>
    private void ToggleInventory(InputAction.CallbackContext ctx)
    {
        bool isInventoryOpen = inventoryAnimator.GetBool("Opened");
        inventoryAnimator.SetBool("Opened", !isInventoryOpen);
    }

    public void CloseAllWindows()
    {
        interfaceAnimator.SetInteger("State", 0);
        inventoryAnimator.SetBool("Opened", false);
        BuildingManager.instance.PlaceBuildingMode = false;
        talentMenuEnviorment.SetActive(false);
    }
}
