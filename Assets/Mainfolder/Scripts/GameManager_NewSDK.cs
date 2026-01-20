// GameManager_NewSDK_Combined.cs
// - 기존 HapticsAndPhysicsEngine 데모(Force toggle / Effector cycle / UI / Texture modes) 유지
// - New SDK VerseGrip 캘리브레이션(Workspace offset/scale) 옵션으로 통합
// - NullReference 방어 + Inverse3 Cursor material 변경

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using DiggingTest;

using Haply.Inverse.DeviceControllers;
using Haply.Inverse.DeviceData;

// 아래 두 컨트롤러는 New SDK VerseGripNavigation 샘플에 있던 것
// 프로젝트에 실제 클래스가 없으면 주석 처리하거나, 해당 컴포넌트를 임포트/추가해야 함.
using Haply.Samples.Experimental.VerseGripNavigation;

namespace Samples.Haply.HapticsAndPhysicsEngine
{
    public class GameManager : MonoBehaviour
    {
        [Header("Devices (New SDK)")]
        public Inverse3Controller inverse3;
        public VerseGripController verseGrip;

        [Header("Optional: Workspace calibration (New SDK)")]
        public WorkspaceOffsetController offsetController;
        public WorkspaceScaleController scaleController;

        [Tooltip("Optional GameObject to visualize the cursor's movement scope (bounds).")]
        public GameObject bounds;

        [Header("Physics")]
        [Range(1, 10000)]
        [Tooltip("Adjust Fixed Timestep directly from here to compare with Physics frequency")]
        public int physicsFrequency = 1000;

        [Header("Effectors")]
        public List<AdvancedPhysicsHapticEffector_NewSDK> advancedEffectors = new();
        private int currentEffectorIndex = 0;

        [Header("Scene / Modes")]
        public GameObject Tutorial;
        public GameObject advance;
        public GameObject default1;

        [Header("Materials")]
        public Material enabledForceMaterial;
        public Material disabledForceMaterial;

        [Header("Knife (optional, used in ActivateAdvance)")]
        public GameObject Knife;

        [Header("UI")]
        public Text helpText;
        public GameObject frequenciesPanel;
        public Text physicsFrequencyText;
        public Image Normal;
        public Image Narrow;
        public Image Triangle;
        public Text hapticsFrequencyText;

        [Header("Messages")]
        public string enableForceMessage = "조각도가 아무 것에도 닿지 않게 이동시키고 SPACE 키를 누르면 힘이 적용됩니다.";
        public string collisionMessage = "Press C to enable/disable collision detection";
        public string backMessage = "T키를 누르면 모드 선택으로 돌아갑니다.";

        [Header("Textures")]
        public Texture advanceTexture;              // 0번 토글용
        [SerializeField] private Texture thirdTexture; // 3번 전환용

        private Texture originalTexture;
        private bool isUsingAdvanceTexture = false;

        private bool forceState = false;

        // -------------------------
        // Unity lifecycle
        // -------------------------

        private void Start()
        {
            // UI / panel safe init
            if (helpText != null) helpText.text = enableForceMessage;
            if (frequenciesPanel != null) frequenciesPanel.SetActive(false);

            // Save original texture
            if (advance != null)
            {
                var renderer = advance.GetComponent<Renderer>();
                if (renderer != null) originalTexture = renderer.material.mainTexture;
            }

            // New SDK: bounds 위치를 workspace center로 맞추기 (device ready 후)
            if (bounds != null && inverse3 != null)
            {
                inverse3.Ready.AddListener((device, _) =>
                {
                    if (bounds != null)
                        bounds.transform.localPosition = device.WorkspaceCenterLocalPosition;
                });
            }

            // calibration 기본 off
            if (offsetController != null) offsetController.enabled = false;
            if (scaleController != null) scaleController.enabled = false;
            if (bounds != null) bounds.SetActive(false);

            // 혹시 effector 활성화 초기화가 필요하면 사용
            InitializeEffectors();
            UpdateImageColorsSafe();
        }

        private void OnEnable()
        {
            if (verseGrip != null)
            {
                verseGrip.ButtonDown.AddListener(OnButtonDown);
                verseGrip.ButtonUp.AddListener(OnButtonUp);
            }
        }

        private void OnDisable()
        {
            if (verseGrip != null)
            {
                verseGrip.ButtonDown.RemoveListener(OnButtonDown);
                verseGrip.ButtonUp.RemoveListener(OnButtonUp);
            }
        }

        private void Update()
        {
            // physics timestep
            if (physicsFrequency > 0)
                Time.fixedDeltaTime = 1f / physicsFrequency;

            // UI frequency text (New SDK에선 hapticThread가 없을 수 있으니 placeholder)
            if (hapticsFrequencyText != null)
                hapticsFrequencyText.text = "haptics : (New SDK)";

            if (physicsFrequencyText != null)
                physicsFrequencyText.text = $"physics : {physicsFrequency}Hz";

            HandleCalibrationKeys();

            // -------------------------
            // Input
            // -------------------------

            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#else
                Application.Quit();
#endif
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleForceFeedback();
                forceState = true;
                // Debug.Log("Force state activated");
                return;
            }

            // 모드 전환: forceState 이후에만 허용 (기존 로직 유지)
            if (forceState && Input.GetKeyDown(KeyCode.Alpha0))
            {
                ActivateDefault();
                return;
            }

            if (forceState && Input.GetKeyDown(KeyCode.Alpha1))
            {
                ActivateTutorial();
                return;
            }

            if (forceState && Input.GetKeyDown(KeyCode.Alpha2))
            {
                ActivateAdvance();
                return;
            }

            if (forceState && Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChangeToThirdTexture();
                return;
            }

            if (forceState && Input.GetKeyDown(KeyCode.T))
            {
                ResetToForceState();
                if (TryGetCurrentEffector(out var eff) && helpText != null)
                    helpText.text = eff.forceEnabled ? collisionMessage : enableForceMessage;
                return;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                CycleEffectors();
                return;
            }
        }

        private void OnGUI()
        {
            if (!TryGetCurrentEffector(out var eff)) return;

            Color textColor = Color.green;

            if (eff.gameObject.activeSelf && eff.touched != null && eff.touched.Count > 0)
            {
                textColor = Color.gray * 0.75f;

                var touchedObject = eff.touched[0];
                if (touchedObject != null)
                {
                    var col = touchedObject.GetComponent<Collider>();
                    if (col != null)
                    {
                        var pm = col.material;
                        var rb = touchedObject.GetComponent<Rigidbody>();

                        string text = $"Collider: {col.name}\n";
                        if (pm != null)
                        {
                            text += $"PhysicsMaterial: {pm.name.Replace("(Instance)", "")}\n" +
                                    $"dynamic friction: {pm.dynamicFriction}, static friction: {pm.staticFriction}\n";
                        }
                        if (rb != null)
                            text += $"mass: {rb.mass}, drag: {rb.drag}, angular drag: {rb.angularDrag}\n";

                        var prev = GUI.color;
                        GUI.color = textColor;
                        GUI.Label(new Rect(20, 40, 800, 200), text);
                        GUI.color = prev;
                    }
                }
                else
                {
                    // null collider cleanup
                    eff.touched.RemoveAt(0);
                }
            }

            // 오른쪽 상단 표시
            {
                var prev = GUI.color;
                GUI.color = textColor;

                var myStyle = new GUIStyle(GUI.skin.label) { fontSize = 24 };
                GUI.Label(new Rect(Screen.width - 220, 20, 200, 30), "Changeable", myStyle);

                GUI.color = prev;
            }

            // 캘리브레이션 안내(있을 때만)
            DrawCalibrationGui();
        }

        // -------------------------
        // Effectors
        // -------------------------

        private void InitializeEffectors()
        {
            if (advancedEffectors == null || advancedEffectors.Count == 0) return;

            for (int i = 0; i < advancedEffectors.Count; i++)
            {
                if (advancedEffectors[i] != null)
                    advancedEffectors[i].gameObject.SetActive(i == 0);
            }

            currentEffectorIndex = Mathf.Clamp(currentEffectorIndex, 0, advancedEffectors.Count - 1);
        }

        private bool TryGetCurrentEffector(out AdvancedPhysicsHapticEffector_NewSDK eff)
        {
            eff = null;
            if (advancedEffectors == null || advancedEffectors.Count == 0) return false;
            if (currentEffectorIndex < 0 || currentEffectorIndex >= advancedEffectors.Count) return false;
            eff = advancedEffectors[currentEffectorIndex];
            // Debug.Log("Current Effector: " + eff.name);
            return eff != null;
        }

        public void CycleEffectors()
        {
            if (!TryGetCurrentEffector(out var current)) return;
            if (current.touched == null) return;

            // 기존 로직 유지: 공중(비접촉) + forceEnabled 상태에서만 교체
            if (current.touched.Count == 0 && current.forceEnabled)
            {
                current.gameObject.SetActive(false);

                currentEffectorIndex = (currentEffectorIndex + 1) % advancedEffectors.Count;

                if (!TryGetCurrentEffector(out var next)) return;
                next.gameObject.SetActive(true);

                // force 상태 유지
                next.forceEnabled = current.forceEnabled;

                // 시각화
                var mr = next.GetComponent<MeshRenderer>();
                if (mr != null) mr.enabled = next.forceEnabled;

                UpdateImageColorsSafe();

                if (helpText != null)
                    helpText.text = next.forceEnabled ? collisionMessage : enableForceMessage;

                ApplyCursorMaterial(next.forceEnabled);
            }
        }

        public void ToggleForceFeedback()
        {
            if (!TryGetCurrentEffector(out var effector)) return;

            effector.forceEnabled = !effector.forceEnabled;

            // effector 시각화 on/off
            var mr = effector.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = effector.forceEnabled;

            // New SDK: cursor material 변경
            ApplyCursorMaterial(effector.forceEnabled);
            // Debug.Log($"Force feedback toggled: {effector.forceEnabled}");

            // UI
            if (helpText != null)
                helpText.text = effector.forceEnabled ? collisionMessage : enableForceMessage;

            UpdateImageColorsSafe();
            // Debug.Log("Force feedback toggled");
        }

        private void ApplyCursorMaterial(bool enabled)
        {
            // 우선 inverse3 필드가 있으면 그걸 사용
            Inverse3Controller inv = inverse3;

            // 없으면 effector의 부모에서 찾기
            if (inv == null && TryGetCurrentEffector(out var eff))
                inv = eff.GetComponentInParent<Inverse3Controller>();

            if (inv == null || inv.Cursor == null) return;

            var cursorMr = inv.Cursor.GetComponent<MeshRenderer>();
            if (cursorMr == null) return;

            var mat = enabled ? enabledForceMaterial : disabledForceMaterial;
            if (mat != null) cursorMr.material = mat;
        }

        private void UpdateImageColorsSafe()
        {
            if (Normal != null) Normal.color = Color.grey;
            if (Narrow != null) Narrow.color = Color.grey;
            if (Triangle != null) Triangle.color = Color.grey;

            switch (currentEffectorIndex)
            {
                case 0: if (Normal != null) Normal.color = Color.green; break;
                case 1: if (Narrow != null) Narrow.color = Color.green; break;
                case 2: if (Triangle != null) Triangle.color = Color.green; break;
            }
        }

        // -------------------------
        // Modes (Tutorial / Advance / Default)
        // -------------------------

        private void ActivateTutorial()
        {
            if (Tutorial != null) Tutorial.SetActive(true);
            if (advance != null) advance.SetActive(false);
            if (default1 != null) default1.SetActive(false);
            if (helpText != null) helpText.text = backMessage;
        }

        private void ActivateDefault()
        {
            ChangeAdvanceTexture();
        }

        private void ActivateAdvance()
        {
            // Knife 부착된 컴포넌트 제거는 optional
            if (Knife != null)
            {
                var drawer = Knife.GetComponent<Drawer>();
                if (drawer != null) Destroy(drawer);

                var dm = Knife.GetComponent<DrawManager>();
                if (dm != null) Destroy(dm);
            }

            if (Tutorial != null) Tutorial.SetActive(false);
            if (default1 != null) default1.SetActive(false);
            if (advance != null) advance.SetActive(true);
            if (helpText != null) helpText.text = backMessage;

            // 모든 effector의 Shovel 활성화(있으면)
            if (advancedEffectors != null)
            {
                foreach (var eff in advancedEffectors)
                {
                    if (eff == null) continue;
                    var shovel = eff.GetComponent<Shovel>();
                    if (shovel != null) shovel.enabled = true;
                }
            }
        }

        private void ResetToForceState()
        {
            if (Tutorial != null) Tutorial.SetActive(false);
            if (advance != null) advance.SetActive(false);
            if (default1 != null) default1.SetActive(false);
        }

        // -------------------------
        // Textures
        // -------------------------

        private void ChangeAdvanceTexture()
        {
            if (advance == null) return;

            var renderer = advance.GetComponent<Renderer>();
            if (renderer == null) return;

            if (isUsingAdvanceTexture)
                renderer.material.mainTexture = originalTexture;
            else
                renderer.material.mainTexture = advanceTexture;

            isUsingAdvanceTexture = !isUsingAdvanceTexture;
        }

        private void ChangeToThirdTexture()
        {
            if (advance == null) return;

            var renderer = advance.GetComponent<Renderer>();
            if (renderer == null) return;

            renderer.material.mainTexture = thirdTexture;
            isUsingAdvanceTexture = false;
        }

        // -------------------------
        // Workspace Calibration (New SDK)
        // -------------------------

        private void HandleCalibrationKeys()
        {
            // 컨트롤러가 없으면 무시
            if (offsetController == null && scaleController == null) return;

            if (Input.GetKeyDown(KeyCode.LeftShift) && offsetController != null) offsetController.enabled = true;
            if (Input.GetKeyUp(KeyCode.LeftShift) && offsetController != null) offsetController.enabled = false;

            if (Input.GetKeyDown(KeyCode.LeftAlt) && scaleController != null) scaleController.enabled = true;
            if (Input.GetKeyUp(KeyCode.LeftAlt) && scaleController != null) scaleController.enabled = false;

            if (bounds != null)
                bounds.SetActive((offsetController != null && offsetController.enabled) ||
                                 (scaleController != null && scaleController.enabled));
        }

        private void OnButtonDown(VerseGripController controller, VerseGripEventArgs args)
        {
            if (offsetController != null) offsetController.enabled = true;
            if (scaleController != null) scaleController.enabled = true;

            if (bounds != null) bounds.SetActive(true);
        }

        private void OnButtonUp(VerseGripController controller, VerseGripEventArgs args)
        {
            if (offsetController != null) offsetController.enabled = false;
            if (scaleController != null) scaleController.enabled = false;

            if (bounds != null) bounds.SetActive(false);
        }

        private void DrawCalibrationGui()
        {
            // 컨트롤러가 없으면 표시 안 함
            if (offsetController == null && scaleController == null) return;

            const float width = 600;
            const float height = 60;
            var rect = new Rect((Screen.width - width) / 2, Screen.height - height - 10, width, height);

            string text = "";
            bool offsetOn = offsetController != null && offsetController.enabled;
            bool scaleOn = scaleController != null && scaleController.enabled;

            if (!offsetOn && !scaleOn)
            {
                text = "Press a VerseGrip BUTTON to calibrate workspace\n" +
                       "(press SHIFT to MOVE only, ALT to SCALE only)";
            }
            if (offsetOn && offsetController != null)
            {
                text += $"Move the Inverse3 cursor to move the workspace : {offsetController.transform.position}\n";
            }
            if (scaleOn && scaleController != null)
            {
                text += $"Rotate the VerseGrip to scale the workspace : ({scaleController.transform.localScale.x:0.000})\n";
            }

            GUI.Box(rect, text, CenteredStyle());
        }

        private static GUIStyle CenteredStyle()
        {
            return new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                fontSize = 14
            };
        }
    }
}
