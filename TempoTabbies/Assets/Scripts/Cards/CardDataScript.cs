using UnityEngine;

public class CardDataScript : MonoBehaviour
{

    public enum CardType
    {
        Buff,       // parantaa omaa pelaajaa
        Debuff,     // heikent‰‰ omaa pelaajaa
        Attack,     // hyˆkk‰‰ vastustajaan
        Defense,    // suojaa itse‰
    }
    public enum EffectType
    {
        ComboLeech
    }
    [CreateAssetMenu(fileName = "NewCard", menuName = "RhythmGame/Card", order = 0)]
    public class CardData : ScriptableObject
    {
        [Header("Perustiedot")]
        public string CardName = "New Card";   // Kortin nimi
        [TextArea] public string description; // Kortin kuvaus
        public EffectType effectType;           // esim. "ComboLeech"
        public Sprite icon;                 // Kuvake
        public CardType type;               // Mink‰ tyyppinen kortti on

        [Header("Pelimekaniikka")]
        public float duration = 10f;        // Kuinka kauan efekti kest‰‰
        public float cooldown = 30f;        // Kuinka usein voi k‰ytt‰‰
        public bool activatesAutomatically = true; // Aktivoituuko automaattisesti 

        [Header("Tehoarvot")]
        public float value;                 // esim. varastettava prosentti
        public float triggerThreshold;      // Ehto, esim. combon ero joka tarvitaan

        [Header("Efektit ja ‰‰ni")]
        public ParticleSystem effectPrefab; // Visuaalinen efekt
    }
}
