using System.Collections.Generic;
using UnityEngine;
using static CardDataScript;
using UnityEngine.InputSystem;

public class CardManagerScript : MonoBehaviour
{
    [Header("Card Database")]
    public List<CardDataScript> AllCards;

    [Header("Player Card Choices")]
    public CardDataScript PlayerAChoice;
    public CardDataScript PlayerBChoice;

    private Gamepad padA;
    private Gamepad padB;
    private int indexA = 0;
    private int indexB = 0;
    private bool lockedA = false;
    private bool lockedB = false;

    void Start()
    {
        if (Gamepad.all.Count > 0) padA = Gamepad.all[0];
        if (Gamepad.all.Count > 1) padB = Gamepad.all[1];
    }

    void Update()
    {
        if (AllCards == null || AllCards.Count == 0) return;

        // --- Player A selection ---
        if (padA != null && !lockedA)
        {
            if (padA.dpad.left.wasPressedThisFrame) indexA = Mathf.Max(0, indexA - 1);
            if (padA.dpad.right.wasPressedThisFrame) indexA = Mathf.Min(AllCards.Count - 1, indexA + 1);

            if (padA.buttonSouth.wasPressedThisFrame) // A button
            {
                PlayerAChoice = AllCards[indexA];
                lockedA = true;
                Debug.Log($"[CardSelect] Player A chose {PlayerAChoice}+ valinta(puuttuu)");
            }
        }

        // --- Player B selection ---
        if (padB != null && !lockedB)
        {
            if (padB.dpad.left.wasPressedThisFrame) indexB = Mathf.Max(0, indexB - 1);
            if (padB.dpad.right.wasPressedThisFrame) indexB = Mathf.Min(AllCards.Count - 1, indexB + 1);

            if (padB.buttonSouth.wasPressedThisFrame)
            {
                PlayerBChoice = AllCards[indexB];
                lockedB = true;

                Debug.Log($"[CardSelect] Player B chose {PlayerBChoice}");
            }
        }

        // Kun molemmat ovat valinneet kortin, peli voi alkaa
        if (lockedA && lockedB)
        {
            Debug.Log("? Both players have chosen cards. Starting match...");
            enabled = false;
        }
    }
}
