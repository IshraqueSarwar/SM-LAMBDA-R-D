using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VirtualScriptEditor : VirtualProgram
{
    
    const string indentString = "   ";
    string legalChar = "abcdefghijklmnopqrstuvwxyz 0.123456789+-/*=<>()[]{},";

    public SyntaxTheme syntaxTheme; // initially run error on this line as SyntaxTheme object isn't created.
    public TMPro.TMP_Text codeUI;
    public TMPro.TMP_Text lineNumberUI;
    //public TMP_Text textInfoUI //WE ARE REMOVING FOR THE MOMENT AS WE PROLLY DON'T NEED IT
    public Image caret;


    public string code {get; set;}
    public int lineIndex;
    public int charIndex;

    public float blinkRate = 1;
    float lastInputTime;
    float blinkTimer;

    // Start is called before the first frame update
    void Start()
    {
        // Checking if no code is entered, therefore we just use an empty string
        if(code ==null){
            code = "";
        }
        // we save the code lenght as charIndex.
        charIndex = code.Length;
        
        

        // We prolly dont need this part as we'll be using a touch Screen
        // But keeping it for now
        // Or if commented, there aint no problem to do so.
        // ?NOTE? IF ONLY TOUCH IS USED STRICTLY WE MIGHT BE ABLE TO 
        //          REMOVE THE CUSTOM INPUT GAMEOBJECT AS WELL. BUT 
        //          IF SOMEONE USES KEYBOARD WITH PHONE/TABLET
        //          WE'll NEED THIS CODE. SO MIGHT AS WELL KEEP IT
        //          AS REDUNDENCY.
        CustomInput.instance.RegisterKey (KeyCode.Backspace);
        CustomInput.instance.RegisterKey (KeyCode.LeftArrow);
        CustomInput.instance.RegisterKey (KeyCode.RightArrow);
        CustomInput.instance.RegisterKey (KeyCode.UpArrow);
        CustomInput.instance.RegisterKey (KeyCode.DownArrow);
        
    }

    // Update is called once per frame
    void Update()
    {
        // If editor isn't active return
        if(!active){
            return;
        }

        // else we continue with handling the input text/code
        HandleTextInput();
        HandleSpecialInput();


    }

    void HandleTextInput(){
        string input = Input.inputString;
        if(!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftCommand)){
            foreach(char c in input){
                if(legalChar.Contains(c.ToString().ToLower())){
                    if(string.IsNullOrEmpty(code) || charIndex == code.Length){
                        code+=c;
                    }else{
                        code = code.Insert(charIndex, c.ToString());
                    }
                    charIndex++;
                }
            }
        }

    }

    void HandleSpecialInput(){

    }
}
