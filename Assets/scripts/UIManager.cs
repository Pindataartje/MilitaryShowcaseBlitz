using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [System.Serializable]
    public class UIAction
    {
        [Tooltip("Keybind to trigger this action")]
        public KeyCode primaryKey = KeyCode.None;

        [Tooltip("Secondary Keybind to trigger this action (optional)")]
        public KeyCode secondaryKey = KeyCode.None;

        [Tooltip("GameObjects to Activate when this key is pressed")]
        public GameObject[] activateObjects;

        [Tooltip("GameObjects to Deactivate when this key is pressed")]
        public GameObject[] deactivateObjects;

        [Tooltip("Animator References for UI Animations")]
        public Animator[] animators;

        [Tooltip("Trigger Animator Parameters (e.g., SlideIn, SlideOut)")]
        public string[] animationTriggers;

        [Header("Player/Camera Control")]
        public bool setPlayerController;
        public bool enablePlayerController;

        public bool setLookCamera;
        public bool enableLookCamera;

        [Header("Cursor State")]
        public bool setCursorState;
        public bool lockAndHideCursor;
    }

    [Header("UI Actions List")]
    public List<UIAction> actions = new List<UIAction>();

    [Header("Player Control References")]
    [SerializeField] PlayerController playerController;
    [SerializeField] SimpleMouseLook simpelLookCamera;

    private void Update()
    {
        foreach (UIAction action in actions)
        {
            if ((action.primaryKey != KeyCode.None && Input.GetKeyDown(action.primaryKey)) ||
                (action.secondaryKey != KeyCode.None && Input.GetKeyDown(action.secondaryKey)))
            {
                HandleUIAction(action);
            }
        }
    }

    private void HandleUIAction(UIAction action)
    {
        if (action.activateObjects != null && action.deactivateObjects != null)
        {
            foreach (GameObject obj in action.activateObjects)
            {
                if (obj != null)
                {
                    if (System.Array.Exists(action.deactivateObjects, element => element == obj))
                    {
                        obj.SetActive(!obj.activeSelf);
                    }
                    else
                    {
                        obj.SetActive(true);
                    }
                }
            }

            foreach (GameObject obj in action.deactivateObjects)
            {
                if (obj != null && !System.Array.Exists(action.activateObjects, element => element == obj))
                {
                    obj.SetActive(false);
                }
            }
        }

        if (action.animators != null && action.animationTriggers != null)
        {
            foreach (Animator animator in action.animators)
            {
                if (animator != null)
                {
                    foreach (string trigger in action.animationTriggers)
                    {
                        if (!string.IsNullOrEmpty(trigger))
                        {
                            animator.ResetTrigger("SlideIn");
                            animator.ResetTrigger("SlideOut");
                            animator.SetTrigger(trigger);
                        }
                    }
                }
            }
        }

        if (action.setPlayerController && playerController != null)
        {
            playerController.enabled = action.enablePlayerController;
        }

        if (action.setLookCamera && simpelLookCamera != null)
        {
            simpelLookCamera.enabled = action.enableLookCamera;
        }

        if (action.setCursorState)
        {
            if (action.lockAndHideCursor)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void EnterUIMode()
    {
        if (playerController != null) playerController.enabled = false;
        if (simpelLookCamera != null) simpelLookCamera.enabled = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ExitUIMode()
    {
        if (playerController != null) playerController.enabled = true;
        if (simpelLookCamera != null) simpelLookCamera.enabled = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
