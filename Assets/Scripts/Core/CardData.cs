using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardData
{
    public string id;
    public string title;
    [TextArea]
    public string description;
    public CardType cardType = CardType.Event;

    // Future: link to audio/video template names
    public string audioTemplateName;  // e.g. "event_default"
    public string videoTemplateName;  // e.g. "event_default"
}
