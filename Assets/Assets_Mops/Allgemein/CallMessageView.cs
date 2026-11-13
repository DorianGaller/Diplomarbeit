using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Eine Nachricht im Phone Call (Vorlage). Aufgebaut fuer die LINKE Seite,
/// der TutorialManager spiegelt sie fuer den Spieler nach rechts.
/// </summary>
public class CallMessageView : MonoBehaviour
{
    public TMP_Text nameText;
    public Image badge;
    public Image ring;
    public TMP_Text bodyText;
}