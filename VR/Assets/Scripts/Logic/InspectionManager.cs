using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectionManager : Singleton<InspectionManager>
{
    private NPCProfile _currentNPC;
    protected override void OnInitialize()
    {
        Debug.Log("[InspectionManager] Initialized");
    }
    
    /// <summary>
    /// 验证NPC的所有证件
    /// </summary>
    public InspectionResult Inspect(NPCProfile npc)
    {
        InspectionResult result = new InspectionResult { isValid = true };
        
        DayRules rules = SessionManager.Instance.currentDayRules;
        
        // 1. 检查护照是否存在
        Document passport = npc.documents.Find(d => d.type == DocumentType.Passport);
        if (passport == null)
        {
            result.AddViolation(ViolationType.MissingDocument);
            result.correctDecision = DecisionType.Deny;
            return result;
        }
        
        // 2. 检查基本信息一致性
        CheckBasicInfo(npc, passport, result);
        
        // 3. 检查日期有效性
        CheckDateValidity(passport, result);
        
        // 4. 检查国籍
        CheckNationality(passport, rules, result);
        
        // 5. 检查工作许可（如果需要）
        if (rules.requireWorkPermit)
        {
            CheckWorkPermit(npc, passport, result);
        }
        
        // 6. 确定正确决策
        DetermineCorrectDecision(result);
        
        return result;
    }
    
    /// <summary>
    /// 检查基本信息
    /// </summary>
    private void CheckBasicInfo(NPCProfile npc, Document passport, InspectionResult result)
    {
        if (passport.holderName != npc.name)
        {
            result.AddViolation(ViolationType.NameMismatch);
        }
        
        if (passport.gender != npc.gender)
        {
            result.AddViolation(ViolationType.GenderMismatch);
        }
        
        if (passport.birthDate != npc.birthDate)
        {
            result.AddViolation(ViolationType.NameMismatch); // 可以扩展为DateMismatch
        }
    }
    
    /// <summary>
    /// 检查日期有效性
    /// </summary>
    private void CheckDateValidity(Document passport, InspectionResult result)
    {
        if (IsDocumentExpired(passport.expiryDate))
        {
            result.AddViolation(ViolationType.Expired);
        }
    }
    
    /// <summary>
    /// 检查国籍
    /// </summary>
    private void CheckNationality(Document passport, DayRules rules, InspectionResult result)
    {
        if (!rules.allowedNationalities.Contains(passport.nationality))
        {
            result.AddViolation(ViolationType.NationalityDenied);
        }
    }
    
    /// <summary>
    /// 检查工作许可
    /// </summary>
    private void CheckWorkPermit(NPCProfile npc, Document passport, InspectionResult result)
    {
        Document workPermit = npc.documents.Find(d => d.type == DocumentType.WorkPermit);
        
        if (workPermit == null)
        {
            result.AddViolation(ViolationType.MissingDocument);
            return;
        }
        
        // 检查工作许可是否过期
        if (IsDocumentExpired(workPermit.expiryDate))
        {
            result.AddViolation(ViolationType.Expired);
        }
        
        // 检查工作许可信息是否匹配
        if (workPermit.holderName != passport.holderName)
        {
            result.AddViolation(ViolationType.NameMismatch);
        }
        
        if (workPermit.documentNumber != passport.documentNumber)
        {
            result.AddViolation(ViolationType.InvalidWorkPermit);
        }
    }
    
    /// <summary>
    /// 判断证件是否过期
    /// </summary>
    private bool IsDocumentExpired(string expiryDate)
    {
        if (string.IsNullOrEmpty(expiryDate)) return true;
        
        try
        {
            string[] parts = expiryDate.Split('.');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            
            // 游戏起始日期：1982年11月23日
            DateTime expiry = new DateTime(year, month, day);
            int currentDay = SessionManager.Instance.currentDay;
            DateTime gameDate = new DateTime(1982, 11, 23).AddDays(currentDay - 1);
            
            return expiry < gameDate;
        }
        catch
        {
            return true;
        }
    }
    
    /// <summary>
    /// 确定正确决策
    /// </summary>
    private void DetermineCorrectDecision(InspectionResult result)
    {
        if (!result.HasViolations)
        {
            result.correctDecision = DecisionType.Approve;
        }
        else
        {
            // 根据违规类型决定应该拒绝还是拘留
            bool hasSeriousViolation = result.violations.Contains(ViolationType.NationalityDenied) ||
                                       result.violations.Contains(ViolationType.MissingDocument);
            
            result.correctDecision = hasSeriousViolation ? DecisionType.Detain : DecisionType.Deny;
        }
    }
    
    /// <summary>
    /// 验证玩家决策是否正确
    /// </summary>
    public bool ValidatePlayerDecision(NPCProfile npc, DecisionType playerDecision)
    {
        InspectionResult result = Inspect(npc);
        bool isCorrect = playerDecision == result.correctDecision;
        
        // 记录决策
        SessionManager.Instance.RecordDecision(isCorrect);
        
        // 发布事件
        EventBus.Publish(EventNames.NPC_PROCESSED, new { npc, playerDecision, isCorrect, result });
        
        Debug.Log($"[InspectionManager] Decision: {playerDecision} | Correct: {result.correctDecision} | Result: {(isCorrect ? "✓" : "✗")}");
        
        return isCorrect;
    }
    public void CallNextNPC()
    {
        if (NPCManager.Instance.HasMoreNPCs())
        {
            _currentNPC = NPCManager.Instance.GetNextNPC();
        
            // 让对话系统开始工作
            DialogueManager.Instance.StartDialogue(_currentNPC);
        
            Debug.Log($"[Inspection] 下一位：{_currentNPC.name}");
        }
        else
        {
            // 没 NPC 了，切换到结算状态
            GameManager.Instance.ChangeState(GameState.DaySettlement);
        }
    }
    public void StartInspection()
    {
        CallNextNPC();
    }
    public void ProcessDecision(int decisionIndex) // 1为批准, 2为拒绝
    {
        if (_currentNPC == null) return;

        DecisionType decision = (DecisionType)decisionIndex;
    
        // 调用你现有的验证逻辑
        bool isCorrect = ValidatePlayerDecision(_currentNPC, decision);
    
        // 获取 NPC 反应文本
        string reaction = DialogueGenerator.GenerateReactionDialogue(decision, _currentNPC.isViolator);
    
        // 让对话框显示 NPC 的遗言/反应
        DialogueManager.Instance.PlaySpecialDialogue(reaction);

        // 延迟一会叫下一个人
        if (isActiveAndEnabled)
        {
            StartCoroutine(WaitAndNext());
        }
        else
        {
            GameManager.Instance.StartCoroutine(WaitAndNext());
        }
    }

    private IEnumerator WaitAndNext()
    {
        yield return new WaitForSeconds(2.0f);
        CallNextNPC();
    }
    
}