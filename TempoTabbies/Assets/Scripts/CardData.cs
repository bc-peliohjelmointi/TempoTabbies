using UnityEngine;

/*[CreateAssetMenu(fileName = "NewCard", menuName = "RhythmGame/Card")]
public class CardDataScript : ScriptableObject
{
    public enum EffectType
    {
        Empty,
        betterHit,
        CatReaper,
        DiscoCat,
        Diva,
        HitChangeBetter,
        HitChangeWorse,
        TapTap,
        VoidCat,
        EightLives,
        Zoomies,
        Loafing,
        CatCompressions
    }

    public enum EffectStatus
    {
        waiting,
        active,
        cooldown
    }

    [Header("Perustiedot")]
    public string CardName = "New Card";   // Kortin nimi
    [TextArea] public string description; // Kortin kuvaus
    public EffectType effectType;           // esim. "betterHit"
    public Sprite icon;                 // Kuvake
                                        // public CardType type;               // Mink‰ tyyppinen kortti on

    [Header("Pelimekaniikka")]
    public float duration = 10f;        // Kuinka kauan efekti kest‰‰
    public float cooldown = 30f;        // Kuinka usein voi k‰ytt‰‰
    public bool activatesAutomatically = true; // Aktivoituuko automaattisesti 

    [field: HideInInspector]
    public bool activeP1 = false;                 // Is the card active or not (turn this on when in a game, and off when you leave the game)
    [field: HideInInspector]
    public bool activeP2 = false;

    [Header("Tehoarvot")]
    public float value;                 // esim. varastettava prosentti
    public float triggerThreshold;      // Ehto, esim. combon ero joka tarvitaan

    [Header("Efektit ja ‰‰ni")]
    public ParticleSystem effectPrefab; // Visuaalinen efekt
    public bool epilepsy;               // Make true if the cards should be turned off in epilepsy mode
}*/