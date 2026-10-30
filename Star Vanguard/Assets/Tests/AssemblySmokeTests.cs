using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using StarVanguard;
using StarVanguard.UI;

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
}
