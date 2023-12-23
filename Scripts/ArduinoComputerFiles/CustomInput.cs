using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomInput : MonoBehaviour
{
    [Tooltip ("Repeat input after cetain duration of time a key is held down.")]
    public float repeatSpeed = .2f;
    public float delayToFirstRepeat = .5f;

    // List of registered keys in their repective 'KeyCode' inputed by the user
    List<KeyCode> registeredKeys = new List<KeyCode>();

    //Dictionary of key and its state in form "KeyCode: KeyState"
    Dictionary<KeyCode, KeyState> keys = new Dictionary<KeyCode, KeyState>();

    public static CustomInput instance;

    public void Awake(){
        instance = this;
        if(FindObjectsOfType<CustomInput>().Length > 1){
            Debug.LogError("Multiple instances of CustomInput.");
        }
    }

    public bool GetKeyPress(KeyCode key){
        Debug.Assert (registeredKeys.Contains (key), "Key not registered");


        if(Input.GetKey(key)){
            KeyState state = keys[key];
            if(Input.GetKeyDown(key)){
                state.lastPhysicalPressTime = Time.time;
                return true;
            }

            // calculate the time since the physical press.
            float timeSinceLastPhysicalPress = Time.time- state.lastPhysicalPressTime;

            //check if last physical press was a while ago and so we need to input the same key again
            if(timeSinceLastPhysicalPress > delayToFirstRepeat){
                // calculate a time for virtual press to replay press
                float timeSinceLastVirtualPress = Time.time - state.lastVirtualPressTime;
                if(timeSinceLastVirtualPress > repeatSpeed){
                    // reset the virtual press time to now so that we can again check for virtual press
                    state.lastVirtualPressTime = Time.time;
                    return true;
                }
            }

        }
        return false;

    }

    public void RegisterKey(KeyCode key){
        if(!registeredKeys.Contains(key)){
            registeredKeys.Add(key);
            KeyState state = new KeyState(key);
            keys.Add(key, state);
        }
    }



    public class KeyState{
        public readonly KeyCode key;
        public float lastVirtualPressTime;
        public float lastPhysicalPressTime;

        public KeyState(KeyCode key){
            this.key = key;
        }
    }
}
