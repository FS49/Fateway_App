using UnityEngine;
using UnityEngine.UI;


public class LightButton : MonoBehaviour
{
[Header("Button Setup")]
public int index; // 0..9
public Image targetImage; // Falls leer, wird automatisch vom Button geholt


private LightsOutManager manager;
private Button uiButton;


void Awake()
{
uiButton = GetComponent<Button>();
if (targetImage == null)
targetImage = GetComponent<Image>();
}


public void Init(LightsOutManager m)
{
manager = m;
uiButton.onClick.RemoveAllListeners();
uiButton.onClick.AddListener(OnPressed);
}


void OnPressed()
{
if (manager == null) return;
manager.OnButtonPressed(index);
}


public void SetInteractable(bool on)
{
if (uiButton) uiButton.interactable = on;
}


public void SetColor(Color c)
{
if (targetImage) targetImage.color = c;
}
}