using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI : MonoBehaviour
{

    public static UI Instance { get; private set; }

    [SerializeField]
    private GameObject menu;

    [SerializeField]
    private GameObject upgradeMenu;

    public PlayerInput input;

    private bool menuScreen = false;

    private bool upgradeScreen = false;

    public static event Action<bool> OnMenuToggle;

    private void Awake()
    {
        input = new PlayerInput();
        Instance = this;
    }

    void Start()
    {
        menuScreen = false;
        upgradeScreen = false;

        menu.SetActive(false);
        upgradeMenu.SetActive(false);

        input.UI.ToggleMenu.performed += (_) => { if (!upgradeScreen) toggleMenu(); };
        input.UI.ToggleMenu.Enable();

        input.UI.ToggleUpgrade.performed += (_) => { if (!menuScreen && (Score.canBuyLevels || upgradeScreen)) toggleUpgrade(); };
        input.UI.ToggleUpgrade.Enable();
    }

    public void toggleUpgrade()
    {
        upgradeScreen = !upgradeScreen;

        OnMenuToggle?.Invoke(upgradeScreen);

        upgradeMenu.SetActive(upgradeScreen);

        Cursor.visible = upgradeScreen;

        if (upgradeScreen)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.Confined;
            input.Player.Disable();
        }
        else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            input.Player.Enable();
        }
    }

    public void toggleMenu()
    {
        menuScreen = !menuScreen;

        OnMenuToggle?.Invoke(menuScreen);

        menu.SetActive(menuScreen);

        Cursor.visible = menuScreen;

        if (menuScreen)
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.Confined;
            input.Player.Disable();
        } else
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            input.Player.Enable();
        }
    }

    public static void CloseGame()
    {
        Application.Quit();
    }

}
