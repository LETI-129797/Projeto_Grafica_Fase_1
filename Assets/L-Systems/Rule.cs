using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Rule
{
    // 1. Predecessor: O símbolo de origem que vai ser substituído
    public char predecessor; 
    
    // 2. Condição: Uma string para diferenciar a quantidade de material
    public string condition;

    // 2. Sucessor: A lista de novos símbolos que vão substituir o predecessor
    public List<Symbol> successor; 


    // Construtor da classe
    public Rule(char predecessor, string condition, List<Symbol> successor)
    {
        this.predecessor = predecessor;
        this.condition = condition;
        this.successor = successor;
    }
    
    [System.NonSerialized]
    public List<Rule> rules = new List<Rule>();

    public void LoadRulesFromFile()
    {
        TextAsset rulesFile = Resources.Load<TextAsset>("Rules");

        if(rulesFile == null)
        {
            Debug.LogError("Não foi possível carregar o arquivo de regras!");
            return;
        }

        string[] lines = rulesFile.text.Split('\n');
        rules.Clear();
        
        foreach (string line in lines)
        {
            string clearLine = line.Trim(); 
            if (string.IsNullOrEmpty(clearLine) || clearLine.StartsWith("//")) continue; // Ignora linhas vazias e comentários

            string[] parts = clearLine.Split(";");

            if(parts.Length == 3)
            {
                char predecessor = parts[0].Trim()[0];
                string condition = parts[1].Trim();
                string successorString = parts[2].Trim();

                List<Symbol> successorList = CreateSymbolList(successorString);

                rules.Add(new Rule(predecessor, condition, successorList));
            }

        }
    }

    private List<Symbol> CreateSymbolList(string input)
    {
        List<Symbol> list = new List<Symbol>();
        
        // Garantir que os parênteses ficam separados por espaços para o Split funcionar bem
        string formattedInput = input.Replace("[", " [ ").Replace("]", " ] ");
        string[] tokens = formattedInput.Split(new[] { ' ' },System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string token in tokens)
        {
            if (token.Length == 1)
            {
                list.Add(new Symbol(token[0]));
            }
            else if (token.Length > 1 )
            {
                char character = token[0];
                string paramString = token.Substring(1);

                if(float.TryParse(paramString, out float paramValue))
                {
                    list.Add(new Symbol(character, "x", paramValue));
                }
                else
                {
                    Debug.LogWarning($"Formato de símbolo inválido: '{token}'");
                }
            }
        }

        return list;
    }
}
