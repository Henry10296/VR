using UnityEngine;

public class PassportUITest : MonoBehaviour
{
    public PassportUI passportUI; // 把你的 PassportUI 拖到这里
    public PassUI passUI;

    void Start()
    {
        // 生成一个测试 NPC
        NPCProfile testNPC = new NPCProfile
        {
            name = "Ivan Petrov",
            gender = "M",
            nationality = "Arstotzka",
            birthDate = "1950.05.10",
            documents = new System.Collections.Generic.List<Document>
            {
                new Document
                {
                    type = DocumentType.Passport,
                    holderName = "Ivan Petrov",
                    gender = "M",
                    nationality = "Arstotzka",
                    documentNumber = "P-123456",
                    issueDate = "1980.01.01",
                    expiryDate = "1985.01.01"
                }
            }
        };

        // 显示护照信息
        passportUI.SetPassportInfo(testNPC);
        passUI.SetPassInfo(testNPC);

    }
}
