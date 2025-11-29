using UnityEngine;

[System.Serializable]
public class CardData
{
    public string id;
    public string title;
    [TextArea]
    public string description;
    public CardType cardType = CardType.Event;
    public string audioTemplateName;
    public string videoTemplateName;
}
