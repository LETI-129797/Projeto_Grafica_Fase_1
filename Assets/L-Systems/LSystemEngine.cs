using UnityEngine;
using System.Collections.Generic;


public class LSystemEngine : MonoBehaviour
{
    public GameObject prefabEstrela;

    public GameObject prefabNuvem;
    
    public GameObject prefabPlaneta;

    public GameObject prefabLua;
    
    public GameObject prefabProtoEstrela;

    public GameObject prefabProtoPlaneta;
    
    public GameObject prefabPlanetesimal;

    public GameObject prefabDiscoPlanetario;

    public GameObject prefabDiscoProtoPlanetario;

    private List<Symbol> currentWord = new List<Symbol>();
    private List<List<Symbol>> history = new List<List<Symbol>>();
    private List<Rule> rules = new List<Rule>();

    void Start()
    {
        LoadRules();
        SetAxiom('n');
    }

    void LoadRules()
    {
        Rule ruleLoader = new Rule(' ', "", new List<Symbol>());
        ruleLoader.LoadRulesFromFile();
        rules = ruleLoader.rules;
    }

    public void SetAxiom(char axiomChar)
    {
        currentWord.Clear();
        history.Clear();
        
        currentWord.Add(new Symbol(axiomChar));
        SaveHistory();
    }

    public void StepForward()
    {
        List<Symbol> nextWord = new List<Symbol>();

        foreach (Symbol sym in currentWord)
        {
            Rule matchedRule = FindMatchingRule(sym);

            if (matchedRule != null)
            {
                // Substitui pelo sucessor da regra
                foreach (Symbol succ in matchedRule.successor)
                {
                    nextWord.Add(succ.Clone());
                }
            }
            else
            {
                // Produção identidade: copia sem alterações
                nextWord.Add(sym.Clone());
            }
        }

        // Verifica se a evolução terminou
        EndEvolution(nextWord);

        currentWord = nextWord;
        SaveHistory();
        PrintCurrentWord();
    }

    public void StepBackward()
    {
        if(history.Count > 1)
        {
            history.RemoveAt(history.Count - 1);

            List<Symbol> previousWord = history[history.Count - 1];
            currentWord.Clear();

            foreach (Symbol s in previousWord)
            {
                currentWord.Add(s.Clone());
            }   
            PrintCurrentWord();
        }
        else
        {
            Debug.Log("Não é possível voltar mais passos. A palavra atual é a inicial.");
        }
    }

    private void EndEvolution(List<Symbol> nextWord)
    {
        if (IsWordEqual(currentWord, nextWord))
        {
            Debug.Log("A evolução do sistema terminou!");
            return;
        }
    }

    private Rule FindMatchingRule(Symbol sym)
    {
        foreach (Rule r in rules)
        {
            if (r.predecessor == sym.character)
            {
                if(EvaluateCondition(r.condition, sym))
                {
                    return r;
                }
            }
        }
        return null; // Aplica regra de identidade se não encontrar
    }

    private bool EvaluateCondition(string condition, Symbol sym)
    {
        if(string.IsNullOrEmpty(condition)) return true; // Se não houver condição, a regra é aplicável

        condition = condition.Trim();

        if (condition.Contains("=="))
        {
            string[] parts = condition.Split(new string[] { "==" }, System.StringSplitOptions.None);
            
            if(parts.Length == 2)
            {
                string paramName = parts[0].Trim();

                if(float.TryParse(parts[1].Trim(), out float value))
                {
                    if(sym.parameters.ContainsKey(paramName) && sym.parameters[paramName] == value)
                    {
                        return true;
                    }
                }
            }
        }          
        return false;
    }
    

    private void SaveHistory()
    {
        List<Symbol> copy = new List<Symbol>();
        foreach (Symbol s in currentWord) copy.Add(s.Clone());
        history.Add(copy);
    }

    private bool IsWordEqual(List<Symbol> w1, List<Symbol> w2)
    {
        if (w1.Count != w2.Count) return false;
        for (int i = 0; i < w1.Count; i++)
        {
            if (w1[i].character != w2[i].character) return false;
        }
        return true;
    }

    private List<Symbol> CreateSymbolList(string input)
    {
        List<Symbol> list = new List<Symbol>();
        string[] tokens = input.Split(' ');
        foreach (string t in tokens)
        {
            if (!string.IsNullOrEmpty(t))
                list.Add(new Symbol(t[0]));
        }
        return list;
    }

    private void PrintCurrentWord()
    {
        string wordStr = "";
        foreach (Symbol s in currentWord) wordStr += s.character;
        Debug.Log("Passo " + (history.Count - 1) + ": " + wordStr);
    }
}
