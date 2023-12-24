using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    
    // public vars editable from the Engine inspector
    public float ZoomSpeed = 0.01f;
    public float touchRotSpeed = 50.0f;
    public float selectionTimer = 1.0f;
    //public GameObject parentModel;
    public GameObject OrbitToggleButton;


    private float prevMag = 0.0f;
    private int touchCount = 0;
    private float pressTime = 0f;// we need this to check when the last time object was touched.
    private Vector3 focusPoint;
    private GameObject selectedObject;
    private string currentObjectName = "";
    private bool orbit = false;
    private bool stillFingerDown = false; // to check if the press is from the deslection round


    
    private Vector3 mousePositionStart;
    private float maxFieldOfView = 160f;
    private float minFieldOfView = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        // START: Touch Pinch To Zoom control
        var scrollaction = new InputAction(binding: "<mouse>/scroll");
        scrollaction.Enable();
        scrollaction.performed += ctx => CameraZoom(ctx.ReadValue<Vector2>().y);

        var touch0contact = new InputAction
            (
            type: InputActionType.Button,
            binding: "<touchscreen>/touch0/press"
            );
        touch0contact.Enable();

        var touch1contact = new InputAction
            (
            type: InputActionType.Button,
            binding: "<touchscreen>/touch1/press"
            );
        touch1contact.Enable();

        touch0contact.performed += _ => touchCount++;
        touch1contact.performed += _ => touchCount++;
        touch0contact.canceled += _ =>
        {
            touchCount--;
            prevMag = 0.0f;
        };

        touch1contact.canceled += _ =>
        {
            touchCount--;
            prevMag = 0.0f;
        };


        var touch0pos = new InputAction
            (
            type: InputActionType.Value,
            binding: "<touchscreen>/touch0/position"
            );
        touch0pos.Enable();

        var touch1pos = new InputAction
            (
            type: InputActionType.Value,
            binding: "<touchscreen>/touch1/position"
            );
        touch1pos.Enable();

        touch1pos.performed += _ =>
        {
            if (touchCount < 2) return;
            var magnitude = (touch0pos.ReadValue<Vector2>() - touch1pos.ReadValue<Vector2>()).magnitude;
            if (prevMag == 0.0f)
            {
                prevMag = magnitude;
            }
            var diff = magnitude - prevMag;
            prevMag = magnitude;
            CameraZoom(-diff * ZoomSpeed);
        };
        // END: Touch Pinch To Zoom control

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 1)
        {
            mousePositionStart = GetPerspectivePos();
            Touch touch0 = Input.GetTouch(0);


            
            if (touch0.phase == UnityEngine.TouchPhase.Stationary)
            {
                // we add delta time to later check if there was a long press on the object
                // therefore selecting it.
                pressTime += Time.deltaTime;

                //We create a raycast to check if our touch collides with an object
                Ray selectionRay = Camera.main.ScreenPointToRay(touch0.position);
                if (Physics.Raycast(selectionRay, out RaycastHit hitObj))
                {
                    
                    // Get the game object
                    // We check if the object is under selectable tag(i.e category)
                    GameObject Object = hitObj.collider.gameObject;
                    if(Object.tag == "Selectable")
                    {
                        // We need to get the container/root name as the base model will be same.
                        string containerName = Object.transform.parent.name;
                        
                        if(containerName != currentObjectName && pressTime >= selectionTimer && !stillFingerDown)//<-- we select an object not selected before after a 'long' press and make sure its not from the deselection touch
                        {
                            // we push the the object name to current object name for later use
                            // We select the object and hightlight it by calling the selectAndOutlineObject coroutine
                            // We focus on the hit object by calling the focusOnHitObject coroutine
                            //resetting orbit button
                            // set stillFingerDown to true to later check to avoid deselecting the object again from the same press--To avoid selection-deselection flicker/loop
                            currentObjectName = containerName;
                            selectAndOutlineObject(Object);
                            focusOnHitObject(hitObj);
                            resetOrbitButtonToFalse();
                            stillFingerDown = true;

                        }
                        else if (containerName == currentObjectName && !stillFingerDown)//<-- if we have touch an already selected object, we deselect it
                        {
                            // DESELECTING THE SELECTED OBJECT(WANTED TO CREATE A SEPARATE FUNC BUT UNNECESSARY IG)
                            // we remove this object from the memory -- selected object
                            // we clear the current object name as we are deselecting it now
                            // we remove the highlight of this object
                            // set stillFingerDown to true to later check to avoid selecting the object again from the same press--To avoid selection-deselection flicker/loop
                            selectedObject = null;
                            currentObjectName = "";
                            Object.GetComponentInParent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
                            stillFingerDown = true;

                        }
                        
                    }
                }
            }

            // We reset the pressTime from the memory so that we can again check for another touch
            if(touch0.phase == UnityEngine.TouchPhase.Ended)
            {
                pressTime = 0f;
                stillFingerDown = false;
            }




            // TOGGLING BETWEEN PAN AND ORBIT
            if (orbit)
            {
                touchOrbit(touch0);
            }
            else
            {
                touchPan(touch0);
            }


            // Making sure that the orbit toggle is only interactable when an object is selected
            if (selectedObject == null)
            {
                OrbitToggleButton.GetComponent<Toggle>().interactable = false;
            }
            else
            {
                OrbitToggleButton.GetComponent<Toggle>().interactable = true;
            }

        }


    }


    void focusOnHitObject(RaycastHit hitObj)
    {
        // We get the focus point--which is the position of the hit object
        // focus() on that point
        focusPoint = hitObj.transform.position;
        Focus(focusPoint);
    }


    void selectAndOutlineObject(GameObject Object)
    {
        // we remove highlight from previously selected object (if any)
        // We push the newly selected object to the memory as-- selected object
        // we highlight the current object that is now in memory as ---selected object
        if (selectedObject != null)
        {
            selectedObject.GetComponentInParent<Outline>().OutlineMode = Outline.Mode.OutlineHidden;
        }
        selectedObject = Object;
        selectedObject.GetComponentInParent<Outline>().OutlineMode = Outline.Mode.OutlineVisible;
    }

    public void resetOrbitButtonToFalse()
    {
        OrbitToggleButton.GetComponent<Toggle>().isOn = false;
    }

    //This function is used by the Orbit Button 
    public void toggleOrbit()
    {
         orbit = !orbit;
         Focus(focusPoint);
    }


    private void touchOrbit(Touch touch)
    {
        if ( touch.deltaPosition.x != 0 || touch.deltaPosition.y != 0)
        {
            float verticleInp = touch.deltaPosition.y * touchRotSpeed * Time.deltaTime;
            float horizontalInp = touch.deltaPosition.x * touchRotSpeed * Time.deltaTime;
       
            transform.Rotate(Vector3.right, -verticleInp);
            transform.Rotate(Vector3.up,horizontalInp, Space.World);

        } 
    }
 


    private Bounds GetBound(GameObject parenGameObj)
    {
        Bounds bound = new Bounds(parenGameObj.transform.position, Vector3.zero);
        var rList = parenGameObj.GetComponentsInChildren(typeof(Renderer));
        foreach(Renderer r in rList)
        {
            bound.Encapsulate(r.bounds);
        }
        return bound;
    }



   
    // TODO: we need to create a camera smoothing.
    public void Focus(Vector3 ToPoint)
    {

        this.transform.position = ToPoint;
    }



    private void touchPan(Touch touch)
    {
        if (touch.deltaPosition.x != 0 || touch.deltaPosition.y != 0)
        {
            Vector3 touchWorldPosDiff = mousePositionStart - GetTouchPerspectivePos(touch);
            transform.position+= touchWorldPosDiff;
            
        }
    }

    

    public Vector3 GetTouchPerspectivePos(Touch touch)
    {
        
        Ray ray = Camera.main.ScreenPointToRay(touch.position+touch.deltaPosition);
        Plane plane = new Plane(transform.forward, 0.0f);
        float dist;
        plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);
    }

    public Vector3 GetPerspectivePos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(transform.forward, 0.0f);
        float dist;
        plane.Raycast(ray, out dist);
        return ray.GetPoint(dist);

    }

   

    private void CameraZoom(float increment) => Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + increment, minFieldOfView, maxFieldOfView);



}
