using UnityEngine;
using TMPro;

/// <summary>
/// 通行证 UI（Entry Permit）
/// </summary>
public class PassUI : MonoBehaviour
{
    [Header("Pass")]
    public TextMeshProUGUI nameLabel;
    public TextMeshProUGUI idLabel;
    public TextMeshProUGUI expiryLabel;

    /// <summary>
    /// 设置通行证信息
    /// </summary>
    public void SetPassInfo(NPCProfile npc)
    {
        if (npc == null || npc.documents == null) return;

        // 查找通行证
        Document pass = npc.documents.Find(d => d.type == DocumentType.EntryPermit);
        if (pass == null)
        {
            Debug.LogWarning("[Pass] NPC 没有通行证（Entry Permit）");
            return;
        }

        nameLabel.text = pass.holderName;
        idLabel.text = pass.documentNumber;
        expiryLabel.text = pass.expiryDate;
    }
}
