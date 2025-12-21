using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 决策类型
/// </summary>
public enum DecisionType
{
    None,
    Approve,   // 批准通过
    Deny,      // 拒绝入境
    Detain     // 拘留
}

/// <summary>
/// 违规类型
/// </summary>
public enum ViolationType
{
    None,
    Expired,           // 证件过期
    NameMismatch,      // 姓名不符
    GenderMismatch,    // 性别不符
    PhotoMismatch,     // 照片不符
    NationalityDenied, // 国籍被拒
    MissingDocument,   // 缺少证件
    FingerprintFailed, // 指纹不符
    InvalidWorkPermit  // 工作许可无效
}

/// <summary>
/// 证件数据
/// </summary>
[System.Serializable]
public class Document
{
    public DocumentType type;
    public string holderName;
    public string gender;           // M/F
    public string nationality;
    public string birthDate;        // YYYY.MM.DD
    public string issueDate;
    public string expiryDate;
    public string documentNumber;
    public Sprite photoSprite;
    public Texture2D fingerprint;
    
    // 工作许可专用
    public string workField;
    
    // 错误标记（用于生成测试数据）
    public bool hasError;
    public ViolationType errorType;
}

/// <summary>
/// NPC数据
/// </summary>
[System.Serializable]
public class NPCProfile
{
    public int id;
    public string name;
    public string gender;
    public string nationality;
    public string birthDate;
    public Sprite faceSprite;
    public Texture2D fingerprint;
    
    // 证件列表
    public List<Document> documents = new List<Document>();
    
    // NPC特性
    public bool isViolator;         // 是否违规
    public ViolationType violationType;
    public bool hasBribe;           // 是否贿赂
    public int bribeAmount;
    
    // 对话内容
    public string greetingText;
    public string purposeText;
    public string pleaText;         // 求情文本
}

/// <summary>
/// 每日规则配置
/// </summary>
[System.Serializable]
public class DayRules
{
    public int dayNumber;
    public string briefingText;
    public List<string> allowedNationalities;
    public bool requireWorkPermit;
    public bool requireFingerprint;
    public bool enableDialogue;
    public int targetNPCCount;      // 目标处理人数
    public string dialogueTemplate;
    public float violatorProbability;
}

/// <summary>
/// 检查结果
/// </summary>
public class InspectionResult
{
    public bool isValid;
    public List<ViolationType> violations = new List<ViolationType>();
    public DecisionType correctDecision; // 正确的决策
    
    public bool HasViolations => violations.Count > 0;
    
    public void AddViolation(ViolationType violation)
    {
        if (!violations.Contains(violation))
        {
            violations.Add(violation);
            isValid = false;
        }
    }
}

/// <summary>
/// 每日结算数据
/// </summary>
[System.Serializable]
public class DaySettlement
{
    public int dayNumber;
    public int processedCount;      // 处理人数
    public int correctCount;        // 正确数
    public int wrongCount;          // 错误数
    public int dailyEarnings;       // 当日收入
    public int dailyExpenses;       // 当日支出
    public int totalMoney;          // 总金额
}

/// <summary>
/// 家庭状态
/// </summary>
[System.Serializable]
public class FamilyStatus
{
    public string wifeCondition;    // 妻子状况
    public string childCondition;   // 孩子状况
    public string fatherCondition;  // 父亲状况
}
