using Haply.HardwareAPI.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DiggingTest; 
namespace Samples.Haply.HapticsAndPhysicsEngine
{
    public class GameManager : MonoBehaviour
    {
        
        [Range(1, 10000)]
        [Tooltip("Adjust Fixed Timestep directly from here to compare with Haptics Thread frequency")]
        public int physicsFrequency = 1000;

        public HapticThread hapticThread;
        //public SimplePhysicsHapticEffector simpleEffector;
        public List<AdvancedPhysicsHapticEffector> advancedEffectors;
        private int currentEffectorIndex = 0; 
        private bool forceState = false;
        public GameObject Tutorial;
        public GameObject advance;
        public Material enabledForceMaterial;
        public Material disabledForceMaterial;
        public GameObject Knife;

        [Header("UI")]
        public Text helpText;
        public GameObject frequenciesPanel;
        public Text physicsFrequencyText;
        public Image Normal;
        public Image Narrow;
        public Image Triangle;
        public Text hapticsFrequencyText;
        public string chooseModeMessage = "Press 2 to Advanced";
        public string enableForceMessage= "조각도가 아무 것에도 닿지 않게 이동시키고 SPACE 키를 누르면 힘이 적용됩니다.";
        public string collisionMessage = "Press C to enable/disable collision detection";
        
        void Start()
        {
            //InitializeEffectors();
            helpText.text = enableForceMessage;
            frequenciesPanel.SetActive(false);
        }
        private void InitializeEffectors()
        {
            for (int i = 0; i < advancedEffectors.Count; i++)
            {
                advancedEffectors[i].gameObject.SetActive(i == 0);
            }
        }
        // Update is called once per frame
        void Update()
        {
            // adjust Fixed Timestep from inspector to compare and understand for demo
            // don't do that in real case, prefer change from ProjectSettings>Time panel
            Time.fixedDeltaTime = 1f / physicsFrequency;

            if (hapticThread.isInitialized)
                hapticsFrequencyText.text = $"haptics : {hapticThread.actualFrequency}Hz";
            physicsFrequencyText.text = $"physics : {physicsFrequency}Hz";



            if ( Input.GetKeyDown( KeyCode.Escape ) )
            {
    #if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
    #else
                Application.Quit();
    #endif
            }

            else if ( Input.GetKeyDown( KeyCode.Space ) )
            {
                ToggleForceFeedback();
                forceState = true;
            }
            else if (!advance.activeSelf && forceState && Input.GetKeyDown(KeyCode.Alpha1))
            {
                ActivateTutorial();
            }
            else if (!Tutorial.activeSelf && forceState && Input.GetKeyDown(KeyCode.Alpha2))
            {
                ActivateAdvance();
            }
            else if (forceState && Input.GetKeyDown(KeyCode.T))
            {
                ResetToForceState();
                helpText.text = advancedEffectors[currentEffectorIndex].forceEnabled ? collisionMessage : enableForceMessage;

            }            // else if ( Input.GetKeyDown( KeyCode.C ) && advancedEffector.gameObject.activeSelf)
            // {
            //     advancedEffector.collisionDetection = !advancedEffector.collisionDetection;
            // }
            // else if ( Input.GetKeyDown( KeyCode.UpArrow ) && frequenciesPanel.activeSelf && physicsFrequency < 10000)
            // {
            //     physicsFrequency += 50;
            // }
            // else if ( Input.GetKeyDown( KeyCode.DownArrow ) && frequenciesPanel.activeSelf && physicsFrequency > 200)
            // {
            //     physicsFrequency -= 100;
            // }
            // else if ( Input.GetKeyDown( KeyCode.DownArrow ) && frequenciesPanel.activeSelf && physicsFrequency > 0)
            // {
            //     physicsFrequency /= 2;
            // }
            // else if ( Input.GetKeyDown( KeyCode.RightArrow ) && frequenciesPanel.activeSelf && hapticThread.targetFrequency < 10000)
            // {
            //     hapticThread.targetFrequency += 50;
            // }
            // else if ( Input.GetKeyDown( KeyCode.LeftArrow ) && frequenciesPanel.activeSelf && hapticThread.targetFrequency > 200)
            // {
            //     hapticThread.targetFrequency -= 100;
            // }
            // else if ( Input.GetKeyDown( KeyCode.LeftArrow ) && frequenciesPanel.activeSelf && hapticThread.targetFrequency > 0)
            // {
            //     hapticThread.targetFrequency /= 2;
            // }
            else if (Input.GetKeyDown(KeyCode.F)) 
            {
                CycleEffectors();
            }
        }
    public void CycleEffectors()
    {
        if (advancedEffectors[currentEffectorIndex].touched.Count == 0 && advancedEffectors[currentEffectorIndex].forceEnabled)
        {
            // 현재 effector 비활성화
            advancedEffectors[currentEffectorIndex].gameObject.SetActive(false);
            
            // 다음 effector로 인덱스 이동 또는 처음으로 순환
            currentEffectorIndex = (currentEffectorIndex + 1) % advancedEffectors.Count;
            advancedEffectors[currentEffectorIndex].gameObject.SetActive(true);

            // 새로 활성화된 effector의 forceEnabled 상태를 이전 effector의 상태로 설정하여 일관성 유지
            advancedEffectors[currentEffectorIndex].forceEnabled = advancedEffectors[(currentEffectorIndex - 1 + advancedEffectors.Count) % advancedEffectors.Count].forceEnabled;
            MeshRenderer currentRenderer = advancedEffectors[currentEffectorIndex].gameObject.GetComponent<MeshRenderer>();
            if (currentRenderer != null) {
                currentRenderer.enabled = true; // 항상 새로 활성화된 effector의 렌더러를 활성화
            }
            // // haptic thread avatar의 재질을 현재 forceEnabled 상태에 따라 업데이트
            // hapticThread.avatar.gameObject.GetComponent<MeshRenderer>().material = 
            //     advancedEffectors[currentEffectorIndex].forceEnabled ? enabledForceMaterial : disabledForceMaterial;
            UpdateImageColors();

            helpText.text = advancedEffectors[currentEffectorIndex].forceEnabled ? collisionMessage : enableForceMessage;
        }
    }

        
        // Display collision infos
        // ----------------------------------
private void OnGUI()
{
    // Set the default color to green
    Color textColor = Color.green;

    if (advancedEffectors[currentEffectorIndex].gameObject.activeSelf &&
        advancedEffectors[currentEffectorIndex].touched.Count > 0)
    {
        // If there is at least one touched object, set the color to dark gray
        textColor = Color.gray * 0.75f; // Dark gray

        // Display information for the first touched object
        var touchedObject = advancedEffectors[currentEffectorIndex].touched[0];
        var collider = touchedObject.GetComponent<Collider>();
        var physicMaterial = collider.material;
        var rb = touchedObject.GetComponent<Rigidbody>();

        string text = $"PhysicsMaterial: {physicMaterial.name.Replace("(Instance)", "")}\n" +
                      $"dynamic friction: {physicMaterial.dynamicFriction}, static friction: {physicMaterial.staticFriction}\n";

        if (rb != null)
        {
            text += $"mass: {rb.mass}, drag: {rb.drag}, angular drag: {rb.angularDrag}\n";
        }

        GUI.Label(new Rect(20, 40, 800, 200), text);
    }

    GUI.color = textColor;

    GUIStyle myStyle = new GUIStyle(GUI.skin.label);
    myStyle.fontSize = 24;  // Set the font size here

    GUI.Label(new Rect(Screen.width - 220, 20, 200, 30), "Changeable", myStyle);

    GUI.color = Color.white;
}
private void UpdateImageColors()
{
    Normal.color = Color.grey;
    Narrow.color = Color.grey;
    Triangle.color = Color.grey;

    switch (currentEffectorIndex)
    {
        case 0:
            Normal.color = Color.green;
            break;
        case 1:
            Narrow.color = Color.green;
            break;
        case 2:
            Triangle.color = Color.green;
            break;
    }
}


        public void ToggleForceFeedback()
        {
            // if ( simpleEffector.gameObject.activeSelf )
            // {
            //     simpleEffector.forceEnabled = !simpleEffector.forceEnabled;
            //     simpleEffector.gameObject.GetComponent<MeshRenderer>().enabled = simpleEffector.forceEnabled;
                    
            //     hapticThread.avatar.gameObject.GetComponent<MeshRenderer>().material =
            //         simpleEffector.forceEnabled ? enabledForceMaterial : disabledForceMaterial;
                    
            //     helpText.text = simpleEffector.forceEnabled ? "" : enableForceMessage;
            // }
            AdvancedPhysicsHapticEffector effector = advancedEffectors[currentEffectorIndex];
            effector.forceEnabled = !effector.forceEnabled;
            effector.gameObject.GetComponent<MeshRenderer>().enabled = effector.forceEnabled;
            hapticThread.avatar.gameObject.GetComponent<MeshRenderer>().material =
            effector.forceEnabled ? enabledForceMaterial : disabledForceMaterial;
            helpText.text = effector.forceEnabled ? collisionMessage : enableForceMessage;
            UpdateImageColors();
        }
        private void ActivateTutorial()
        {
            Tutorial.SetActive(true);
            advance.SetActive(false);
            helpText.text = "T키를 누르면 모드 선택으로 돌아갑니다.";
        }

        private void ActivateAdvance()
        {
            Destroy(Knife.GetComponent<Drawer>());
            Destroy(Knife.GetComponent<DrawManager>());
            Tutorial.SetActive(false);
            advance.SetActive(true);
            helpText.text = "T키를 누르면 모드 선택으로 돌아갑니다.";

            // 현재 advanced effector의 shovel 컴포넌트를 활성화
            foreach (var effector in advancedEffectors)
            {
                Shovel shovel = effector.GetComponent<Shovel>();
                if (shovel != null)
                {
                    shovel.enabled = true;
                }
            }
        }

        private void ResetToForceState()
        {
            Tutorial.SetActive(false);
            advance.SetActive(false);
        }
    }
}
