using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Symbol
{
    // O carácter que identifica o símbolo (ex: 'n', 'p', 'e', 'D', '1', '[', ']')
    public char character; 

    // Dicionário com os parâmetros associados ao símbolo (ex: r = raio, x = material)
    
    [SerializeField] public Dictionary<string, float> parameters; 

    // Construtor principal
    public Symbol(char character)
    {
        this.character = character;
        this.parameters = new Dictionary<string, float>();
    }

    // Construtor utilitário para criar símbolos com parâmetros
    public Symbol(char character, string paramName, float paramValue)
    {
        this.character = character;
        this.parameters = new Dictionary<string, float>();
        this.parameters.Add(paramName, paramValue);
    }

    // Método para criar uma cópia exata (clone) do símbolo
    public Symbol Clone()
    {
        Symbol copy = new Symbol(this.character);
        foreach (KeyValuePair<string, float> kvp in this.parameters)
        {
            copy.parameters[kvp.Key] = kvp.Value;
        }
        return copy;
    }
}