using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConfigManager : Singleton<ConfigManager>
{
    [Header("游戏平衡")]
    public int correctReward = 5;       // 正确奖励
    public int wrongPenalty = -5;       // 错误惩罚
    public int dailyExpenses = 20;      // 每日开销
    public int initialMoney = 50;       // 初始金额
    
    [Header("NPC配置")]
    public float npcArriveDelay = 2f;   // NPC到达延迟
    public float npcDepartDelay = 1f;   // NPC离开延迟
    
    [Header("VR配置")]
    public float grabDistance = 0.5f;   // 抓取距离
    public float interactDistance = 1f; // 交互距离
    
    protected override void OnInitialize()
    {
        Debug.Log("[ConfigManager] Initialized");
    }
    
    /// <summary>
    /// 获取每日规则
    /// </summary>
    public DayRules GetDayRules(int day)
    {
        // 这里可以从JSON/ScriptableObject加载
        // 目前使用硬编码
        switch (day)
        {
            case 1:
                return new DayRules
                {
                    dayNumber = 1,
                    briefingText = "Welcome to Grestin Checkpoint No. 6. Today only basic passport checks are required.",
                    allowedNationalities = new List<string> { "Arstotzka" },
                    requireWorkPermit = false,
                    requireFingerprint = false,
                    enableDialogue = true,
                    targetNPCCount = 10
                };
                
            case 2:
                return new DayRules
                {
                    dayNumber = 2,
                    briefingText = "Starting today, foreign citizens must provide a work permit and fingerprint verification.",
                    allowedNationalities = new List<string> { "Arstotzka", "Obristan" },
                    requireWorkPermit = true,
                    requireFingerprint = true,   
                    enableDialogue = true,
                    targetNPCCount = 12
                };
                
            default:
                return new DayRules
                {
                    dayNumber = day,
                    briefingText = "Continue your work.",
                    allowedNationalities = new List<string> { "Arstotzka" },
                    requireWorkPermit = false,
                    requireFingerprint = false,
                    enableDialogue = false,
                    targetNPCCount = 10
                };
        }
    }
}
