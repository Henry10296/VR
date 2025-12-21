using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionManager : Singleton<SessionManager>
{
    
    [Header("会话数据")]
    public int currentDay = 0;
    public int totalMoney = 50;  
    public int processedNPCCount = 0;
    public int correctDecisions = 0;
    public int wrongDecisions = 0;
    public int dailyEarnings = 0;
    
    public DayRules currentDayRules;
    public FamilyStatus familyStatus;
    
    protected override void OnInitialize()
    {
        familyStatus = new FamilyStatus
        {
            wifeCondition = "Health",
            childCondition = "hung",
            fatherCondition = "康复中"
        };
        Debug.Log("[SessionManager] Initialized");
    }
    
    /// <summary>
    /// 开始新的一天
    /// </summary>
    public void StartNewDay(int day)
    {
        currentDay = day;
        processedNPCCount = 0;
        correctDecisions = 0;
        wrongDecisions = 0;
        dailyEarnings = 0;
        
        // 加载当日规则
        currentDayRules = ConfigManager.Instance.GetDayRules(day);
        
        Debug.Log($"[SessionManager] Day {day} started. Target: {currentDayRules.targetNPCCount} NPCs");
    }
    
    /// <summary>
    /// 记录决策结果
    /// </summary>
    public void RecordDecision(bool isCorrect)
    {
        processedNPCCount++;
        
        if (isCorrect)
        {
            correctDecisions++;
            int reward = ConfigManager.Instance.correctReward;
            dailyEarnings += reward;
            totalMoney += reward;
        }
        else
        {
            wrongDecisions++;
            int penalty = ConfigManager.Instance.wrongPenalty;
            dailyEarnings += penalty;
            totalMoney += penalty;
        }
        
        EventBus.Publish(EventNames.MONEY_CHANGED, totalMoney);
        
        Debug.Log($"[SessionManager] Processed: {processedNPCCount}/{currentDayRules.targetNPCCount} | " +
                  $"Correct: {correctDecisions} | Wrong: {wrongDecisions} | Money: {totalMoney}");
    }
    
    /// <summary>
    /// 检查是否完成当日工作
    /// </summary>
    public bool IsDayComplete()
    {
        return processedNPCCount >= currentDayRules.targetNPCCount;
    }
    
    /// <summary>
    /// 结算当天
    /// </summary>
    public DaySettlement SettleDay()
    {
        // 扣除每日开销
        int expenses = ConfigManager.Instance.dailyExpenses;
        totalMoney -= expenses;
        
        // 更新家庭状态
        UpdateFamilyStatus();
        
        DaySettlement settlement = new DaySettlement
        {
            dayNumber = currentDay,
            processedCount = processedNPCCount,
            correctCount = correctDecisions,
            wrongCount = wrongDecisions,
            dailyEarnings = dailyEarnings,
            dailyExpenses = expenses,
            totalMoney = totalMoney
        };
        
        EventBus.Publish(EventNames.UI_SHOW_SETTLEMENT, settlement);
        
        Debug.Log($"[SessionManager] Day {currentDay} settled. Net: {dailyEarnings - expenses} | Total: {totalMoney}");
        
        return settlement;
    }
    
    /// <summary>
    /// 更新家庭状态
    /// </summary>
    private void UpdateFamilyStatus()
    {
        // 根据金钱和表现更新家庭状态
        if (totalMoney < 20)
        {
            familyStatus.childCondition = "饥饿";
        }
        else
        {
            familyStatus.childCondition = "饱食";
        }
        
        if (wrongDecisions > 3)
        {
            familyStatus.wifeCondition = "生病";
        }
        else if (totalMoney > 40)
        {
            familyStatus.wifeCondition = "健康";
        }
        
        if (totalMoney < 10)
        {
            familyStatus.fatherCondition = "病重";
        }
        else if (totalMoney > 50)
        {
            familyStatus.fatherCondition = "康复中";
        }
        
        EventBus.Publish(EventNames.FAMILY_STATUS_UPDATED, familyStatus);
    }
    
    /// <summary>
    /// 重置会话
    /// </summary>
    public void ResetSession()
    {
        currentDay = 0;
        totalMoney = ConfigManager.Instance.initialMoney;
        processedNPCCount = 0;
        correctDecisions = 0;
        wrongDecisions = 0;
        dailyEarnings = 0;
        
        familyStatus = new FamilyStatus
        {
            wifeCondition = "健康",
            childCondition = "饱食",
            fatherCondition = "康复中"
        };
    }
}
