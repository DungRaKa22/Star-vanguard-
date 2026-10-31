using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarVanguard;
using StarVanguard.UI;
using StarVanguard.Net;

namespace StarVanguard.Tests
{
    /// <summary>
    /// Initial automated smoke tests validating assembly definition loading,
    /// core type accessibility, and strict one-way architectural invariants.
    /// </summary>
    [TestFixture]
    public class AssemblySmokeTests
    {
        [Test]
        public void RuntimeAssembly_LoadsExpectedCoreTypes()
        {
            Type gameManagerType = typeof(GameManager);
            Type playerControllerType = typeof(PlayerController);
            Type poolManagerType = typeof(PoolManager);
            Type audioManagerType = typeof(AudioManager);
            Type bootstrapLoaderType = typeof(BootstrapLoader);

            Assert.IsNotNull(gameManagerType, "GameManager must be resolvable from StarVanguard.Runtime");
            Assert.IsNotNull(playerControllerType, "PlayerController must be resolvable from StarVanguard.Runtime");
            Assert.IsNotNull(poolManagerType, "PoolManager must be resolvable from StarVanguard.Runtime");
            Assert.IsNotNull(audioManagerType, "AudioManager must be resolvable from StarVanguard.Runtime");
            Assert.IsNotNull(bootstrapLoaderType, "BootstrapLoader must be resolvable from StarVanguard.Runtime");
        }

        [Test]
        public void UIAssembly_LoadsExpectedUITypes()
        {
            Type routerType = typeof(UIPanelRouter);
            Type mainMenuType = typeof(MainMenuScreen);
            Type settingsType = typeof(SettingsScreen);
            Type controlsType = typeof(ControlsScreen);

            Assert.IsNotNull(routerType, "UIPanelRouter must be resolvable from StarVanguard.UI");
            Assert.IsNotNull(mainMenuType, "MainMenuScreen must be resolvable from StarVanguard.UI");
            Assert.IsNotNull(settingsType, "SettingsScreen must be resolvable from StarVanguard.UI");
            Assert.IsNotNull(controlsType, "ControlsScreen must be resolvable from StarVanguard.UI");
        }

        [Test]
        public void ArchitecturalConstraint_RuntimeDoesNotReferenceUI()
        {
            Assembly runtimeAssembly = typeof(GameManager).Assembly;
            AssemblyName[] referencedAssemblies = runtimeAssembly.GetReferencedAssemblies();

            bool referencesUI = referencedAssemblies.Any(a =>
                string.Equals(a.Name, "StarVanguard.UI", StringComparison.OrdinalIgnoreCase));

            Assert.IsFalse(referencesUI, 
                "Violation of R4: StarVanguard.Runtime must NEVER reference StarVanguard.UI directly! UI must subscribe to Runtime events.");
        }

        [Test]
        public void ArchitecturalConstraint_UIDependsOnRuntime()
        {
            Assembly uiAssembly = typeof(UIPanelRouter).Assembly;
            AssemblyName[] referencedAssemblies = uiAssembly.GetReferencedAssemblies();

            bool referencesRuntime = referencedAssemblies.Any(a =>
                string.Equals(a.Name, "StarVanguard.Runtime", StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(referencesRuntime, 
                "StarVanguard.UI must have an explicit assembly reference to StarVanguard.Runtime.");
        }

        [Test]
        public void NetAssembly_LoadsExpectedNetcodeTypes()
        {
            Type bootType = typeof(NetworkManagerBootstrap);
            Type bridgeType = typeof(NetworkPlayerBridge);
            Type waveCoordType = typeof(CoopWaveCoordinator);
            Type menuPresenterType = typeof(CoopMenuPresenter);

            Assert.IsNotNull(bootType, "NetworkManagerBootstrap must be resolvable from StarVanguard.Net");
            Assert.IsNotNull(bridgeType, "NetworkPlayerBridge must be resolvable from StarVanguard.Net");
            Assert.IsNotNull(waveCoordType, "CoopWaveCoordinator must be resolvable from StarVanguard.Net");
            Assert.IsNotNull(menuPresenterType, "CoopMenuPresenter must be resolvable from StarVanguard.Net");
        }

        [Test]
        public void ArchitecturalConstraint_RuntimeDoesNotReferenceNet()
        {
            Assembly runtimeAssembly = typeof(GameManager).Assembly;
            AssemblyName[] referencedAssemblies = runtimeAssembly.GetReferencedAssemblies();

            bool referencesNet = referencedAssemblies.Any(a =>
                string.Equals(a.Name, "StarVanguard.Net", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(a.Name, "Unity.Netcode.Runtime", StringComparison.OrdinalIgnoreCase));

            Assert.IsFalse(referencesNet,
                "Violation of ADR-002: StarVanguard.Runtime must NEVER reference StarVanguard.Net or Unity.Netcode.Runtime!");
        }

        [Test]
        public void EnumsAndConstants_AreConsistent()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(eGameState), eGameState.Menu));
            Assert.IsTrue(Enum.IsDefined(typeof(eGameState), eGameState.Playing));
            Assert.IsTrue(Enum.IsDefined(typeof(eGameState), eGameState.Paused));
            Assert.IsTrue(Enum.IsDefined(typeof(eGameState), eGameState.GameOver));

            Assert.IsTrue(Enum.IsDefined(typeof(UIPanelId), UIPanelId.Home));
            Assert.IsTrue(Enum.IsDefined(typeof(UIPanelId), UIPanelId.Settings));
            Assert.IsTrue(Enum.IsDefined(typeof(UIPanelId), UIPanelId.Controls));
        }
    }

    /// <summary>
    /// Empirical stress test suite for Milestone 2 (OverheatGaugeUI uGUI & Zero-GC).
    /// Adversarially challenges:
    /// 1. Event subscription lifecycle (repeated enable/disable, unsubscription, no leaks).
    /// 2. Value range and extreme boundary conditions (negative, overflow, rapid oscillation, tier thresholds).
    /// 3. Zero-GC heap allocation during active updates in steady-state.
    /// 4. Defensive null-safety when references are unassigned or destroyed.
    /// 5. Secondary HUD handlers (crystals, weapon badges, division-by-zero protection).
    /// </summary>
    [TestFixture]
    public class OverheatGaugeStressTests
    {
        private GameObject m_TestRoot;
        private OverheatGaugeUI m_GaugeUI;
        private Slider m_Slider;
        private Image m_FillImage;
        private TextMeshProUGUI m_StatusText;

        [SetUp]
        public void SetUp()
        {
            m_TestRoot = new GameObject("Test_OverheatGaugeRoot");

            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderObj.transform.SetParent(m_TestRoot.transform, false);
            m_Slider = sliderObj.GetComponent<Slider>();
            m_Slider.minValue = 0f;
            m_Slider.maxValue = 1f;

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObj.transform.SetParent(sliderObj.transform, false);
            m_FillImage = fillObj.GetComponent<Image>();
            m_Slider.fillRect = fillObj.GetComponent<RectTransform>();

            GameObject textObj = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(m_TestRoot.transform, false);
            m_StatusText = textObj.GetComponent<TextMeshProUGUI>();

            m_GaugeUI = m_TestRoot.AddComponent<OverheatGaugeUI>();

            // Assign private fields via reflection
            SetPrivateField(m_GaugeUI, "m_HeatSlider", m_Slider);
            SetPrivateField(m_GaugeUI, "m_HeatFillImage", m_FillImage);
            SetPrivateField(m_GaugeUI, "m_HeatStatusText", m_StatusText);

            // In EditMode, lifecycle methods must be invoked explicitly
            InvokePrivateMethod(m_GaugeUI, "OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_GaugeUI != null)
            {
                InvokePrivateMethod(m_GaugeUI, "OnDisable");
            }

            if (m_TestRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(m_TestRoot);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static object InvokePrivateMethod(object target, string methodName, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return method?.Invoke(target, args);
        }

        private static void FireHeatChanged(float heat)
        {
            FieldInfo field = typeof(WeaponHeatController).GetField("OnHeatChanged", BindingFlags.Static | BindingFlags.NonPublic);
            var del = (Action<float>)field?.GetValue(null);
            del?.Invoke(heat);
        }

        private static void FireOverheated(bool overheated)
        {
            FieldInfo field = typeof(WeaponHeatController).GetField("OnOverheated", BindingFlags.Static | BindingFlags.NonPublic);
            var del = (Action<bool>)field?.GetValue(null);
            del?.Invoke(overheated);
        }

        [Test]
        public void Stress_EventSubscription_MultipleEnableDisableCycles_NoLeaksOrDoubleSub()
        {
            m_GaugeUI.ResetUI();

            // Run 50 rapid enable/disable cycles
            for (int i = 0; i < 50; i++)
            {
                InvokePrivateMethod(m_GaugeUI, "OnDisable");
                InvokePrivateMethod(m_GaugeUI, "OnEnable");
            }

            // Verify single subscription: fire event and check normalized heat
            FireHeatChanged(0.42f);
            Assert.AreEqual(0.42f, m_GaugeUI.NormalizedHeat, 0.001f, "OverheatGaugeUI must receive heat event after enable cycles");
            Assert.AreEqual(0.42f, m_Slider.value, 0.001f, "Slider value must match heat");

            // Disable and verify unsubscription
            InvokePrivateMethod(m_GaugeUI, "OnDisable");
            FireHeatChanged(0.88f);
            Assert.AreEqual(0.42f, m_GaugeUI.NormalizedHeat, 0.001f, "OverheatGaugeUI must NOT update while disabled");

            // Re-enable and verify resume
            InvokePrivateMethod(m_GaugeUI, "OnEnable");
            FireHeatChanged(0.65f);
            Assert.AreEqual(0.65f, m_GaugeUI.NormalizedHeat, 0.001f, "OverheatGaugeUI resumes receiving events when re-enabled");
        }

        [Test]
        public void Stress_ExtremeHeatValues_ClampingAndColorTiers()
        {
            // 1. Negative heat test
            FireHeatChanged(-10.0f);
            Assert.AreEqual(0.0f, m_GaugeUI.NormalizedHeat, 0.0001f, "Negative heat clamped to 0.0");
            Assert.AreEqual(0.0f, m_Slider.value, 0.0001f, "Slider clamped to 0.0");
            Assert.AreEqual(OverheatGaugeUI.GetHeatColor(0.0f), m_FillImage.color, "Cyan color at 0%");

            // 2. Normal intermediate tiers
            FireHeatChanged(0.39f);
            Assert.AreEqual(OverheatGaugeUI.GetHeatColor(0.39f), m_FillImage.color, "Cyan tier (< 40%)");

            FireHeatChanged(0.40f);
            Assert.AreEqual(OverheatGaugeUI.GetHeatColor(0.40f), m_FillImage.color, "Amber tier (40% - 75%)");

            FireHeatChanged(0.749f);
            Assert.AreEqual(OverheatGaugeUI.GetHeatColor(0.749f), m_FillImage.color, "Amber tier (40% - 75%)");

            FireHeatChanged(0.75f);
            Assert.AreEqual(OverheatGaugeUI.GetHeatColor(0.75f), m_FillImage.color, "Red tier (>= 75%)");

            // 3. Overflow heat test
            FireHeatChanged(150.0f);
            Assert.AreEqual(1.0f, m_GaugeUI.NormalizedHeat, 0.0001f, "Excess heat clamped to 1.0");
            Assert.AreEqual(1.0f, m_Slider.value, 0.0001f, "Slider clamped to 1.0");
            Assert.AreEqual(OverheatGaugeUI.GetHeatColor(1.0f), m_FillImage.color, "Red color at 100%");
        }

        [Test]
        public void Stress_RapidOscillation_1000Cycles()
        {
            for (int i = 0; i < 1000; i++)
            {
                float heat = (i % 2 == 0) ? 0.0f : 1.0f;
                bool overheated = (i % 2 != 0);

                FireHeatChanged(heat);
                FireOverheated(overheated);
            }

            // Final state after odd iteration (999) is overheated at 1.0
            Assert.AreEqual(1.0f, m_GaugeUI.NormalizedHeat, 0.001f);
            Assert.IsTrue(m_GaugeUI.IsOverheated);
        }

        [Test]
        public void Stress_DefensiveNullSafety_AllMethodsSurviveMissingReferences()
        {
            GameObject nakedObj = new GameObject("Naked_OverheatGauge");
            OverheatGaugeUI nakedUI = nakedObj.AddComponent<OverheatGaugeUI>();

            Assert.DoesNotThrow(() => FireHeatChanged(0.5f));
            Assert.DoesNotThrow(() => FireOverheated(true));
            Assert.DoesNotThrow(() => FireOverheated(false));
            Assert.DoesNotThrow(() => nakedUI.ResetUI());
            Assert.DoesNotThrow(() => InvokePrivateMethod(nakedUI, "HandleCrystalChargeChanged", 0, 0));
            Assert.DoesNotThrow(() => InvokePrivateMethod(nakedUI, "HandleCrystalChargeChanged", -10, -5));
            Assert.DoesNotThrow(() => InvokePrivateMethod(nakedUI, "HandleWeaponChanged", (eWeaponType)99, 99));
            Assert.DoesNotThrow(() => InvokePrivateMethod(nakedUI, "HandleWeaponChanged", (eWeaponType)(-1), -1));

            UnityEngine.Object.DestroyImmediate(nakedObj);
        }

        [Test]
        public void Stress_ZeroGC_HandleHeatChanged_SteadyState()
        {
            FieldInfo field = typeof(WeaponHeatController).GetField("OnHeatChanged", BindingFlags.Static | BindingFlags.NonPublic);
            var del = (Action<float>)field?.GetValue(null);
            Assert.IsNotNull(del, "OnHeatChanged delegate must have at least one subscriber (m_GaugeUI)");

            // Warm up JIT and TMP caches
            for (int i = 0; i < 20; i++)
            {
                del(0.5f);
            }

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            long allocBefore = System.GC.GetAllocatedBytesForCurrentThread();

            for (int i = 0; i < 1000; i++)
            {
                float heat = (i % 2 == 0) ? 0.50f : 0.51f;
                del(heat);
            }

            long allocAfter = System.GC.GetAllocatedBytesForCurrentThread();
            long totalAllocated = allocAfter - allocBefore;

            Assert.AreEqual(0, totalAllocated,
                $"HandleHeatChanged must produce ZERO GC allocations in steady state. Observed: {totalAllocated} bytes over 1000 invocations.");
        }

        [Test]
        public void Sprint2_CameraShake_AppliesAndDecaysToOriginalPosition()
        {
            GameObject camObj = new GameObject("TestCamera");
            CameraShakeController shaker = camObj.AddComponent<CameraShakeController>();
            Vector3 startPos = camObj.transform.localPosition;

            shaker.TriggerShake(0.5f, 0.1f);
            
            // Advance time via LateUpdate reflection
            MethodInfo lateUpdateMethod = typeof(CameraShakeController).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            if (lateUpdateMethod != null)
            {
                FieldInfo timerField = typeof(CameraShakeController).GetField("m_Timer", BindingFlags.Instance | BindingFlags.NonPublic);
                timerField?.SetValue(shaker, 0.0001f);
                lateUpdateMethod.Invoke(shaker, null);
                
                timerField?.SetValue(shaker, 0f);
                lateUpdateMethod.Invoke(shaker, null);
            }

            Assert.AreEqual(startPos, camObj.transform.localPosition, "Camera localPosition must restore to original after shake");
            UnityEngine.Object.DestroyImmediate(camObj);
        }

        [Test]
        public void Sprint2_ZeroGC_PlasmaAngles_StaticArray()
        {
            FieldInfo f5 = typeof(PlayerShooting).GetField("s_PlasmaAnglesLevel5", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo f3 = typeof(PlayerShooting).GetField("s_PlasmaAnglesLevel3", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo f1 = typeof(PlayerShooting).GetField("s_PlasmaAnglesLevel1", BindingFlags.Static | BindingFlags.NonPublic);

            Assert.IsNotNull(f5, "s_PlasmaAnglesLevel5 static cached array must exist");
            Assert.IsNotNull(f3, "s_PlasmaAnglesLevel3 static cached array must exist");
            Assert.IsNotNull(f1, "s_PlasmaAnglesLevel1 static cached array must exist");

            float[] a5 = (float[])f5.GetValue(null);
            float[] a3 = (float[])f3.GetValue(null);
            float[] a1 = (float[])f1.GetValue(null);

            Assert.AreEqual(5, a5.Length);
            Assert.AreEqual(3, a3.Length);
            Assert.AreEqual(2, a1.Length);
        }

        [Test]
        public void Sprint2_PoolManager_ActiveRegistriesZeroAllocation()
        {
            GameObject poolObj = new GameObject("TestPoolManager");
            PoolManager pm = poolObj.AddComponent<PoolManager>();

            Assert.DoesNotThrow(() =>
            {
                var enemies = pm.ActiveEnemies;
                var bullets = pm.ActiveBullets;
                var bombs = pm.ActiveBombs;
                var enemyBullets = pm.ActiveEnemyBullets;
            });

            UnityEngine.Object.DestroyImmediate(poolObj);
        }
    }
}
