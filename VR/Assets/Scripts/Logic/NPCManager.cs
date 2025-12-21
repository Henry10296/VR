using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class NPCManager : Singleton<NPCManager>
{
    [Header("NPC队列")]
    private Queue<NPCProfile> _npcQueue = new Queue<NPCProfile>();
    private NPCProfile _currentNPC;
    
    [Header("名字数据库")]
    private string[] _maleNames = { "Ivan Petrov", "Dmitri Volkov", "Sergei Antonov", "Boris Kozlov", "Viktor Sokolov" };
    private string[] _femaleNames = { "Maria Kowalski", "Anna Volkov", "Elena Ivanova", "Natasha Romanova", "Olga Petrova" };
    private string[] _nationalities = { "Arstotzka", "Obristan", "Kolechia", "Antegria", "Republia", "Impor" };
    
    protected override void OnInitialize()
    {
        Debug.Log("[NPCManager] Initialized");
    }
    
    /// <summary>
    /// 生成今日NPC队列
    /// </summary>
    public IEnumerator GenerateDailyNPCs()
    {
        _npcQueue.Clear();
        
        DayRules rules = SessionManager.Instance.currentDayRules;
        int count = rules.targetNPCCount;
        
        for (int i = 0; i < count; i++)
        {
            NPCProfile npc = GenerateNPC(i, rules);
            DialogueGenerator.GenerateDialogueForNPC(npc);
            _npcQueue.Enqueue(npc);
        }
        
        Debug.Log($"[NPCManager] Generated {count} NPCs for Day {SessionManager.Instance.currentDay}");
        yield return null;
    }
    
    /// <summary>
    /// 获取下一个NPC
    /// </summary>
    public NPCProfile GetNextNPC()
    {
        if (_npcQueue.Count > 0)
        {
            _currentNPC = _npcQueue.Dequeue();
            EventBus.Publish(EventNames.NPC_ARRIVED, _currentNPC);
            Debug.Log($"[NPCManager] NPC Arrived: {_currentNPC.name} from {_currentNPC.nationality}");
            return _currentNPC;
        }
        
        Debug.LogWarning("[NPCManager] No more NPCs in queue");
        return null;
    }
    
    /// <summary>
    /// 获取当前NPC
    /// </summary>
    public NPCProfile GetCurrentNPC()
    {
        return _currentNPC;
    }
    
    /// <summary>
    /// 是否还有待处理的NPC
    /// </summary>
    public bool HasMoreNPCs()
    {
        return _npcQueue.Count > 0;
    }
    
    /// <summary>
    /// 生成单个NPC
    /// </summary>
    private NPCProfile GenerateNPC(int id, DayRules rules)
    {
        // 随机性别
        string gender = UnityEngine.Random.value > 0.5f ? "M" : "F";
        
        // 随机姓名
        string name = gender == "M" 
            ? _maleNames[UnityEngine.Random.Range(0, _maleNames.Length)]
            : _femaleNames[UnityEngine.Random.Range(0, _femaleNames.Length)];
        
        // 随机国籍
        string nationality = _nationalities[UnityEngine.Random.Range(0, _nationalities.Length)];
        
        // 随机生日
        string birthDate = GenerateRandomDate(1940, 1965);
        
        NPCProfile npc = new NPCProfile
        {
            id = id,
            name = name,
            gender = gender,
            nationality = nationality,
            birthDate = birthDate,
            greetingText = "Good morning.",
            purposeText = "I'm here to work."
        };
        
        // 生成证件
        GenerateDocuments(npc, rules);
        
        // 根据当日规则的违规概率
        if (UnityEngine.Random.value < rules.violatorProbability)
        {
            IntroduceViolation(npc, rules);
        }
        
        return npc;
    }
    
    /// <summary>
    /// 生成证件
    /// </summary>
    private void GenerateDocuments(NPCProfile npc, DayRules rules)
    {
        // 护照（必需）
        Document passport = new Document
        {
            type = DocumentType.Passport,
            holderName = npc.name,
            gender = npc.gender,
            nationality = npc.nationality,
            birthDate = npc.birthDate,
            issueDate = GenerateRandomDate(1975, 1980),
            expiryDate = GenerateRandomDate(1983, 1990),
            documentNumber = $"P-{UnityEngine.Random.Range(100000, 999999)}"
        };
        npc.documents.Add(passport);
        
        // 工作许可（根据规则）
        if (rules.requireWorkPermit)
        {
            Document workPermit = new Document
            {
                type = DocumentType.WorkPermit,
                holderName = npc.name,
                documentNumber = passport.documentNumber,
                expiryDate = GenerateRandomDate(1983, 1985),
                workField = GetRandomWorkField()
            };
            npc.documents.Add(workPermit);
        }
    }
    
    /// <summary>
    /// 引入违规
    /// </summary>
    private void IntroduceViolation(NPCProfile npc, DayRules rules)
    {
        npc.isViolator = true;
        
        // 随机选择违规类型
        ViolationType[] possibleViolations = {
            ViolationType.Expired,
            ViolationType.NameMismatch,
            ViolationType.GenderMismatch,
            ViolationType.NationalityDenied,
            ViolationType.MissingDocument
        };
        
        ViolationType violation = possibleViolations[UnityEngine.Random.Range(0, possibleViolations.Length)];
        npc.violationType = violation;
        
        Document passport = npc.documents.Find(d => d.type == DocumentType.Passport);
        if (passport == null) return;
        
        switch (violation)
        {
            case ViolationType.Expired:
                passport.expiryDate = GenerateRandomDate(1979, 1981);
                passport.hasError = true;
                passport.errorType = violation;
                break;
                
            case ViolationType.NameMismatch:
                passport.holderName = npc.gender == "M" 
                    ? _maleNames[UnityEngine.Random.Range(0, _maleNames.Length)]
                    : _femaleNames[UnityEngine.Random.Range(0, _femaleNames.Length)];
                passport.hasError = true;
                passport.errorType = violation;
                break;
                
            case ViolationType.GenderMismatch:
                passport.gender = passport.gender == "M" ? "F" : "M";
                passport.hasError = true;
                passport.errorType = violation;
                break;
                
            case ViolationType.NationalityDenied:
                // 选择不在允许列表中的国籍
                do
                {
                    passport.nationality = _nationalities[UnityEngine.Random.Range(0, _nationalities.Length)];
                } while (rules.allowedNationalities.Contains(passport.nationality));
                passport.hasError = true;
                passport.errorType = violation;
                break;
                
            case ViolationType.MissingDocument:
                if (rules.requireWorkPermit && npc.documents.Count > 1)
                {
                    npc.documents.RemoveAt(1); // 移除工作许可
                }
                break;
        }
    }
    
    /// <summary>
    /// 生成随机日期
    /// </summary>
    private string GenerateRandomDate(int startYear, int endYear)
    {
        int year = UnityEngine.Random.Range(startYear, endYear + 1);
        int month = UnityEngine.Random.Range(1, 13);
        int day = UnityEngine.Random.Range(1, 29);
        return $"{year}.{month:D2}.{day:D2}";
    }
    
    /// <summary>
    /// 获取随机工作领域
    /// </summary>
    private string GetRandomWorkField()
    {
        string[] fields = { "Agriculture", "Industry", "Service", "Construction", "Transportation" };
        return fields[UnityEngine.Random.Range(0, fields.Length)];
    }
}
