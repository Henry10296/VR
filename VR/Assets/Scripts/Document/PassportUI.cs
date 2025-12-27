using UnityEngine;
using TMPro;

public class PassportUI : MonoBehaviour
{
    [Header("护照字段")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI idLabel;
    public TextMeshProUGUI genderLabel;
    public TextMeshProUGUI nationalityLabel;
    public TextMeshProUGUI issuePlaceLabel;
    public TextMeshProUGUI expiryLabel;

    public void SetPassportInfo(NPCProfile npc)
    {
        if (npc == null || npc.documents == null) return;

        Document passport = npc.documents.Find(d => d.type == DocumentType.Passport);
        if (passport == null) return;

        nameLabel.text = passport.holderName;
        idLabel.text =  passport.documentNumber;
        genderLabel.text =  passport.gender == "M" ? "Male" : "Female";
        nationalityLabel.text =  passport.nationality;
        issuePlaceLabel.text =  passport.nationality; 
        expiryLabel.text = passport.expiryDate;
    }
}
