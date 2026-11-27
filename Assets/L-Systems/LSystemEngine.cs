using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;


public class LSystemEngine : MonoBehaviour
{

    [Header("Configuração Orbital")]
    public float orbitRadiusIncrement = 5f; // Valor 'r' configurável pela interface
    public float baseOrbitSpeed = 30f;      // Velocidade base dos planetas
    public float moonOrbitRadius = 1.2f;    // Distância base da lua ao planeta
    public float moonOrbitSpeed = 80f;      // Velocidade das luas

    private List<GameObject> spawnedObjects = new List<GameObject>();
    public GameObject prefabEstrela;

    public GameObject prefabNuvem;
    
    public GameObject prefabPlaneta;

    public GameObject prefabLua;
    
    public GameObject prefabProtoEstrela;

    public GameObject prefabProtoPlaneta;
    
    public GameObject prefabPlanetesimal;

    public GameObject prefabDiscoPlanetario;

    public GameObject prefabDiscoProtoPlanetario;

    public bool isEvolutionFinished = false;

    private List<Symbol> currentWord = new List<Symbol>();
    private List<List<Symbol>> history = new List<List<Symbol>>();
    
    [SerializeReference]
    private List<Rule> rules = new List<Rule>();

    void Update()
{
    if (Keyboard.current == null) return;

    // Seta Direita ou Espaço para avançar um passo
    if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
    {
        StepForward();
    }

    // Seta Esquerda para recuar um passo
    if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
    {
        StepBackward();
    }
}

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
        isEvolutionFinished = false;
        currentWord.Clear();
        history.Clear();
        
        currentWord.Add(new Symbol(axiomChar));
        SaveHistory();
        RenderCurrentWord();
    }

    public void StepForward()
    {

        if (isEvolutionFinished)
    {
        Debug.Log("Avanço desativado: a evolução já terminou.");
        return;
    }
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
        if(EndEvolution(nextWord))
        {
            return;
        }

        currentWord = nextWord;
        SaveHistory();
        PrintCurrentWord();
        RenderCurrentWord();
    }

    public void StepBackward()
    {
        if(history.Count > 1)
        {
            isEvolutionFinished = false;
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
        RenderCurrentWord();
    }

    private bool EndEvolution(List<Symbol> nextWord)
    {
        if (IsWordEqual(currentWord, nextWord))
        {
            isEvolutionFinished = true;
            Debug.Log("A evolução do sistema terminou!");
            return true;
        }
        return false;
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
            //Verifica se os caracteres são diferentes
            if (w1[i].character != w2[i].character) return false;
            
            //Se um parametro tiver um valor diferente do outro, então as palavras não são iguais
            if(w1[i].parameters.Count != w2[i].parameters.Count) return false;

            //Verifica se todos os parâmetros são iguais
            foreach(KeyValuePair<string,float> kvp in w1[i].parameters)
            {
                if(!w2[i].parameters.TryGetValue(kvp.Key, out float otherValue) || kvp.Value != otherValue)
                {
                    return false;
                }
            }
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

    public void RenderCurrentWord()
    {
        ClearScene();

        Stack<GameObject> contextStack = new Stack<GameObject>();
        GameObject centralStar = null;     // Guarda a referência da estrela/proto-estrela
        GameObject lastCreatedBody = null; // Guarda o último corpo para a pilha '['
        
        int orbitIndex = 1; // Índice i para o raio ri (1, 2, 3...)
        int moonIndex = 0;  // Conta luas do mesmo planeta para variar raio/velocidade

        foreach (Symbol sym in currentWord)
        {
            GameObject spawned = null;

            switch (sym.character)
            {
                case 'n': // Nuvem inicial no centro
                    spawned = Instantiate(prefabNuvem, Vector3.zero, Quaternion.identity);
                    spawned.transform.localScale = Vector3.one * (orbitRadiusIncrement * 3f);
                    break;

                case 'p': // Proto-estrela no centro
                    spawned = Instantiate(prefabProtoEstrela, Vector3.zero, Quaternion.identity);
                    centralStar = spawned; // Passa a ser o alvo orbital dos corpos seguintes
                    break;

                case 'e': // Estrela madura no centro
                    spawned = Instantiate(prefabEstrela, Vector3.zero, Quaternion.identity);
                    centralStar = spawned; // Passa a ser o alvo orbital dos corpos seguintes
                    break;

                case 'd': // Disco protoplanetário à volta da estrela
                    spawned = Instantiate(prefabDiscoProtoPlanetario, Vector3.zero, Quaternion.identity);
                    if (sym.parameters.TryGetValue("x", out float matValue))
                    {
                        float diskScale = matValue * orbitRadiusIncrement * 0.8f;
                        spawned.transform.localScale = new Vector3(diskScale, 1f, diskScale);
                    }
                    break;

                case 'D': // Anel de material na órbita ri
                case 'M': // Planetesimal na órbita ri
                case 'N': // Proto-planeta na órbita ri
                case 'P': // Planeta na órbita ri
                    GameObject prefabToUse = GetOrbitalPrefab(sym.character);
                    float currentRadius = orbitIndex * orbitRadiusIncrement;

                    Vector3 spawnPosition = new Vector3(currentRadius, 0f, 0f);

                    spawned = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
                    
                    // Configura o script Orbit do teu colega
                    Orbit planetOrbit = spawned.GetComponent<Orbit>();
                    if (planetOrbit != null && centralStar != null)
                    {
                        planetOrbit.target = centralStar.transform;
                        planetOrbit.radius = currentRadius;
                        // Planetas mais afastados orbitam ligeiramente mais devagar (opcional, dá realismo)
                        planetOrbit.speed = baseOrbitSpeed / Mathf.Sqrt(orbitIndex); 
                        planetOrbit.SetInitialPosition();
                    }

                    orbitIndex++; // Prepara o raio para o próximo corpo orbital
                    break;

                case '[': // Abre ramo: guarda o corpo atual (ex: Planeta P) na pilha
                    if (lastCreatedBody != null)
                    {
                        contextStack.Push(lastCreatedBody);
                        moonIndex = 0;
                    }
                    break;

                case ']': // Fecha ramo: regressa ao contexto anterior
                    if (contextStack.Count > 0)
                    {
                        contextStack.Pop();
                    }
                    break;

                case 'l': // Lua em órbita do corpo no topo da pilha
                    if (contextStack.Count > 0)
                    {
                        GameObject parentPlanet = contextStack.Peek();
                        spawned = Instantiate(prefabLua, Vector3.zero, Quaternion.identity);
                        
                        // Configura o script Orbit para rodar à volta do planeta pai
                        Orbit moonOrbit = spawned.GetComponent<Orbit>();
                        if (moonOrbit != null)
                        {
                            moonOrbit.target = parentPlanet.transform;
                            // Aumenta ligeiramente o raio para a 2ª lua não colidir com a 1ª
                            moonOrbit.radius = moonOrbitRadius + (moonIndex * 0.6f);
                            moonOrbit.speed = moonOrbitSpeed * (moonIndex % 2 == 0 ? 1f : 0.8f);
                            moonOrbit.SetInitialPosition();
                        }

                        moonIndex++;
                    }
                    break;
            }

            if (spawned != null)
            {
                spawnedObjects.Add(spawned);
                lastCreatedBody = spawned;
            }
        }
    }

    private void ClearScene()
    {
    foreach (GameObject obj in spawnedObjects)
    {
        if (obj != null)
        {
            Destroy(obj);
        }
    }
    spawnedObjects.Clear();
    }

    private GameObject GetOrbitalPrefab(char c)
    {
        switch (c)
        {
            case 'D': return prefabDiscoPlanetario;
            case 'M': return prefabPlanetesimal;
            case 'N': return prefabProtoPlaneta;
            case 'P': return prefabPlaneta;
            default: return null;
        }
    }

        public int GetCurrentStep()
    {
        return history.Count - 1;
    }

    public string GetCurrentWordString()
    {
        string wordStr = "";
        foreach (Symbol s in currentWord)
        {
            wordStr += s.character;
            if (s.parameters.ContainsKey("x"))
                wordStr += s.parameters["x"];
            wordStr += " ";
        }
        return wordStr.Trim();
    }

    public void RestartWithRadius(float newRadius)
    {
        orbitRadiusIncrement = newRadius;
        SetAxiom('n');
    }

}
