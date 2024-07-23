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
        public GameObject default1;

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
        public string enableForceMessage = "조각도가 아무 것에도 닿지 않게 이동시키고 SPACE 키를 누르면 힘이 적용됩니다.";
        public string collisionMessage = "Press C to enable/disable collision detection";

        [Header("Textures")]
        public Texture advanceTexture;  // 추가된 텍스처 변수
        private Texture originalTexture;  // 원래 텍스처를 저장할 변수
        private bool isUsingAdvanceTexture = false;  // 현재 어떤 텍스처를 사용 중인지 추적하는 변수

        void Start()
        {
            //InitializeEffectors();
            helpText.text = enableForceMessage;
            frequenciesPanel.SetActive(false);

            // Start 메서드에서 원래 텍스처를 저장
            Renderer renderer = advance.GetComponent<Renderer>();
            if (renderer != null)
            {
                originalTexture = renderer.material.mainTexture;
            }
        }

        private void InitializeEffectors()
        {
            for (int i = 0; i < advancedEffectors.Count; i++)
            {
                advancedEffectors[i].gameObject.SetActive(i == 0);
            }
        }

        void Update()
        {
            Time.fixedDeltaTime = 1f / physicsFrequency;

            if (hapticThread.isInitialized)
                hapticsFrequencyText.text = $"haptics : {hapticThread.actualFrequency}Hz";
            physicsFrequencyText.text = $"physics : {physicsFrequency}Hz";

            if (Input.GetKeyDown(KeyCode.Escape))
            {
    #if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
    #else
                Application.Quit();
    #endif
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleForceFeedback();
                forceState = true;
            }
            else if (!default1.activeSelf && forceState && Input.GetKeyDown(KeyCode.Alpha0))
            {
                ActivateDefault();
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
            }
            else if (Input.GetKeyDown(KeyCode.F)) 
            {
                CycleEffectors();
            }
        }

        public void CycleEffectors()
        {
            if (advancedEffectors[currentEffectorIndex].touched.Count == 0 && advancedEffectors[currentEffectorIndex].forceEnabled)
            {
                advancedEffectors[currentEffectorIndex].gameObject.SetActive(false);
                currentEffectorIndex = (currentEffectorIndex + 1) % advancedEffectors.Count;
                advancedEffectors[currentEffectorIndex].gameObject.SetActive(true);
                advancedEffectors[currentEffectorIndex].forceEnabled = advancedEffectors[(currentEffectorIndex - 1 + advancedEffectors.Count) % advancedEffectors.Count].forceEnabled;
                MeshRenderer currentRenderer = advancedEffectors[currentEffectorIndex].gameObject.GetComponent<MeshRenderer>();
                if (currentRenderer != null) {
                    currentRenderer.enabled = true;
                }
                UpdateImageColors();
                helpText.text = advancedEffectors[currentEffectorIndex].forceEnabled ? collisionMessage : enableForceMessage;
            }
        }

        private void OnGUI()
        {
            Color textColor = Color.green;

            if (advancedEffectors[currentEffectorIndex].gameObject.activeSelf &&
                advancedEffectors[currentEffectorIndex].touched.Count > 0)
            {
                textColor = Color.gray * 0.75f;

                var touchedObject = advancedEffectors[currentEffectorIndex].touched[0];

                if (touchedObject != null)
                {
                    var collider = touchedObject.GetComponent<Collider>();
                    if (collider != null)
                    {
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
                }
                else
                {
                    advancedEffectors[currentEffectorIndex].touched.RemoveAt(0);
                }
            }

            GUI.color = textColor;

            GUIStyle myStyle = new GUIStyle(GUI.skin.label);
            myStyle.fontSize = 24;

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
            default1.SetActive(false);
            helpText.text = "T키를 누르면 모드 선택으로 돌아갑니다.";
        }

        private void ActivateDefault()
        {
            ChangeAdvanceTexture();  // 0번 키를 눌렀을 때 텍스처 변경
        }

        private void ActivateAdvance()
        {
            Destroy(Knife.GetComponent<Drawer>());
            Destroy(Knife.GetComponent<DrawManager>());
            Tutorial.SetActive(false);
            default1.SetActive(false);
            advance.SetActive(true);
            helpText.text = "T키를 누르면 모드 선택으로 돌아갑니다.";
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
            default1.SetActive(false);
        }

        private void ChangeAdvanceTexture()
        {
            Renderer renderer = advance.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (isUsingAdvanceTexture)
                {
                    renderer.material.mainTexture = originalTexture;
                }
                else
                {
                    renderer.material.mainTexture = advanceTexture;
                }
                isUsingAdvanceTexture = !isUsingAdvanceTexture;  // 상태를 반전시킴
            }
        }
    }
}
