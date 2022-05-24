using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerStats : MonoBehaviourPun
{
    public enum ClassNames { SCOUT, TANK, SUPPORT, BOMBER }
    [SerializeField] private ClassNames currentClass;
    public ClassNames CurrentClass { get { return currentClass; } }
    [SerializeField] private int health = 100;
    public int Health { get { return health; } }
    [SerializeField] private int maxHealth = 100;
    private int currentMaxHealth = 100;
    [SerializeField] private float shieldHealth = 50;
    [SerializeField] private int startingShieldHealth = 50;
    [SerializeField] private float regenDelay = 1;
    [SerializeField] private float regenSpeed = 0.25f;
    [SerializeField] private Transform classModels;
    public Transform ClassModels { get { return classModels; } }
    public bool HasShieldHealth { get; set; } = false;

    private PlayerUI playerUI;
    private PlayerRevive playerRevive;
    public PlayerShoot PlayerShootScript { get; private set; }
    public PlayerHealing PlayerHealingScript { get; private set; }
    private Animator anim;
    private int healthIncreaseCounter = 0;
    private bool isDown = false;
    public bool IsDown { get { return isDown; } }
    public bool IsDead { get; set; } = false;

    public delegate void Interact(PlayerStats playerStats);
    public Interact OnInteract;
    [SerializeField] private List<InteractableObject> inventoryOfInteractables = new List<InteractableObject>();
    public List<InteractableObject> InventoryOfInteractables { get { return inventoryOfInteractables; } }

    private void Awake()
    {
        if (PlayerChoiceManager.instance != null)
        {
            foreach (int item in PlayerChoiceManager.instance.ChosenClasses.Keys)
            {
                Debug.Log("Actor: " + (photonView.OwnerActorNr - 1) +  " KeyNmr: " + item);
            }
            int playerIndex = PhotonNetwork.IsConnected ? photonView.OwnerActorNr - 1 : 0;
            currentClass = PlayerChoiceManager.instance.ChosenClasses[playerIndex];
        }
    }

    private void Start()
    {
        shieldHealth = startingShieldHealth;
        health = maxHealth;
        currentMaxHealth = maxHealth;
        anim = GetComponentInChildren<Animator>();
        PlayerShootScript = GetComponent<PlayerShoot>();
        PlayerHealingScript = GetComponent<PlayerHealing>();

        classModels.GetChild((int)currentClass).gameObject.SetActive(true);

        if (photonView.IsMine || !PhotonNetwork.IsConnected)
        {
            SetLayerRecursively(classModels.GetChild((int)currentClass).gameObject, 10);

            playerUI = FindObjectOfType<PlayerUI>();

            if (currentClass != ClassNames.TANK)
            {
                playerUI.HideShieldHealth();
            }

            playerUI.UpdateHealth(health, maxHealth);
            //playerUI.UpdateMaxHealth(maxHealth);
            playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);
            playerRevive = GetComponent<PlayerRevive>();
        }      
    }

    public void PickUpInteractable(InteractableObject interactable)
    {
        inventoryOfInteractables.Add(interactable);
    }

    public void RemoveInteractable(InteractableObject interactable)
    {
        inventoryOfInteractables.Remove(interactable);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child == null || child.GetComponent<SpriteRenderer>() != null)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public void Heal(int healthGain)
    {
        health += healthGain;

        if (health >= currentMaxHealth)
        {
            health = currentMaxHealth;
        }

        playerUI.UpdateHealth(health, maxHealth);
    }

    public bool PlayerAtMaxHealth()
    {
        return health >= currentMaxHealth;
    }

    public void Damage(int dmg)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        playerUI.ShowBloodScreen();

        if (isDown)
        {
            playerRevive.ResetDamageTimer();
            return;
        }

        if (HasShieldHealth && shieldHealth > 0)
        {
            shieldHealth -= dmg;

            bool moreDamageThanShield = false;

            if (shieldHealth < 0)
            {
                moreDamageThanShield = true;

                dmg += Mathf.RoundToInt(shieldHealth);

                shieldHealth = 0;
            }

            playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);

            StopCoroutine(nameof(StartShieldRegeneration));
            StartCoroutine(nameof(StartShieldRegeneration));

            if (!moreDamageThanShield)
            {
                return;
            }            
        }

        health -= dmg;

        if (health <= 0)
        {
            health = 0;
            isDown = true;
            anim.SetBool("Down", true);
            playerRevive.StartTimer();
            StopCoroutine(nameof(StartShieldRegeneration));
            playerUI.UpdateShieldHealth(0, startingShieldHealth);
        }

        playerUI.UpdateHealth(health, maxHealth);
    }

    private IEnumerator StartShieldRegeneration()
    {
        yield return new WaitForSeconds(regenDelay);

        while (shieldHealth < startingShieldHealth)
        {
            shieldHealth += regenSpeed * Time.deltaTime;

            if (shieldHealth > startingShieldHealth)
            {
                shieldHealth = startingShieldHealth;
            }

            playerUI.UpdateShieldHealth(shieldHealth, startingShieldHealth);

            yield return null;
        }
    }

    public void IncreaseMaxHealth()
    {
        healthIncreaseCounter++;

        currentMaxHealth = Mathf.RoundToInt(maxHealth * (1 + (0.25f * (float)healthIncreaseCounter)));

        //playerUI.UpdateMaxHealth(currentMaxHealth);
    }
}