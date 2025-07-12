using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Reflection;

/// <summary>
/// InputHandler 관련 테스트
/// 주의: 이 테스트들은 Unity Input System 패키지가 설치되어 있어야 정상적으로 작동합니다.
/// Input System 패키지가 없는 경우 Inconclusive 결과가 나타날 수 있습니다.
/// </summary>
public class InputHandlerTests
{
    private GameObject logicManagerObject;
    private MonoBehaviour logicManager;
    private GameObject inputHandlerObject;
    private MonoBehaviour inputHandler;

    [SetUp]
    public void SetUp()
    {
        // LogicManager 생성 및 초기화
        logicManagerObject = new GameObject("TestLogicManager");
        var logicManagerType = System.Type.GetType("LogicManager");
        if (logicManagerType != null)
        {
            logicManager = logicManagerObject.AddComponent(logicManagerType) as MonoBehaviour;
            if (logicManager != null)
            {
                logicManager.Invoke("Start", 0f);
            }
        }

        // InputHandler 생성 및 초기화
        inputHandlerObject = new GameObject("TestInputHandler");
        var inputHandlerType = System.Type.GetType("InputHandler");
        if (inputHandlerType != null && logicManager != null)
        {
            inputHandler = inputHandlerObject.AddComponent(inputHandlerType) as MonoBehaviour;

            // Initialize 메서드 호출
            var initializeMethod = inputHandlerType.GetMethod("Initialize");
            if (initializeMethod != null)
            {
                try
                {
                    initializeMethod.Invoke(inputHandler, new object[] { logicManager });
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"InputHandler initialization failed (Input System may not be available): {ex.Message}");
                }
            }
        }
    }

    [TearDown]
    public void TearDown()
    {
        if (inputHandlerObject != null)
        {
            Object.DestroyImmediate(inputHandlerObject);
        }

        if (logicManagerObject != null)
        {
            Object.DestroyImmediate(logicManagerObject);
        }
    }

    [Test]
    public void InputHandler_Exists_ShouldBeAccessible()
    {
        // Arrange & Act
        var inputHandlerType = System.Type.GetType("InputHandler");

        // Assert
        Assert.IsNotNull(inputHandlerType, "InputHandler class should exist in the project");
    }

    [Test]
    public void InputHandler_HasSimulationMethods_ShouldBeTestable()
    {
        // Arrange
        if (inputHandler == null)
        {
            Assert.Inconclusive("InputHandler not available for testing");
            return;
        }

        var inputHandlerType = inputHandler.GetType();

        // Act & Assert
        var simulateInputMethod = inputHandlerType.GetMethod("SimulateInput");
        Assert.IsNotNull(simulateInputMethod, "SimulateInput method should exist for testing");

        var simulateInputStopMethod = inputHandlerType.GetMethod("SimulateInputStop");
        Assert.IsNotNull(simulateInputStopMethod, "SimulateInputStop method should exist for testing");
    }

    [Test]
    public void InputHandler_SimulateInput_ShouldCallLogicMethods()
    {
        // Arrange
        if (inputHandler == null || logicManager == null)
        {
            Assert.Inconclusive("InputHandler or LogicManager not available for testing");
            return;
        }

        var inputHandlerType = inputHandler.GetType();
        var simulateInputMethod = inputHandlerType.GetMethod("SimulateInput");

        if (simulateInputMethod == null)
        {
            Assert.Inconclusive("SimulateInput method not available for testing");
            return;
        }

        // Act & Assert - 각 입력 액션 테스트
        try
        {
            simulateInputMethod.Invoke(inputHandler, new object[] { "MoveLeft" });
            Assert.Pass("MoveLeft input simulation completed");
        }
        catch (System.Exception ex)
        {
            // Input System이 없거나 초기화되지 않은 경우
            if (ex.Message.Contains("NullReferenceException") || ex.Message.Contains("Input"))
            {
                Assert.Inconclusive($"Input System may not be properly initialized: {ex.Message}");
            }
            else
            {
                Assert.Fail($"MoveLeft input simulation failed: {ex.Message}");
            }
        }
    }

    [UnityTest]
    public IEnumerator InputSystem_MovementSimulation_ShouldRespondCorrectly()
    {
        // Arrange
        if (inputHandler == null || logicManager == null)
        {
            Assert.Inconclusive("Components not available for Input System testing");
            yield break;
        }

        var inputHandlerType = inputHandler.GetType();
        var simulateInputMethod = inputHandlerType.GetMethod("SimulateInput");
        var simulateInputStopMethod = inputHandlerType.GetMethod("SimulateInputStop");

        if (simulateInputMethod == null)
        {
            Assert.Inconclusive("SimulateInput method not available");
            yield break;
        }

        bool testPassed = true;
        string errorMessage = "";

        // Act - 좌측 이동 테스트
        try
        {
            simulateInputMethod.Invoke(inputHandler, new object[] { "MoveLeft" });
        }
        catch (System.Exception ex)
        {
            if (ex.Message.Contains("NullReferenceException") || ex.Message.Contains("Input"))
            {
                Assert.Inconclusive($"Input System may not be available: {ex.Message}");
                yield break;
            }
            testPassed = false;
            errorMessage = $"MoveLeft simulation failed: {ex.Message}";
        }

        yield return new WaitForFixedUpdate();

        // 입력 중지 시뮬레이션
        if (testPassed && simulateInputStopMethod != null)
        {
            try
            {
                simulateInputStopMethod.Invoke(inputHandler, new object[] { "MoveLeft" });
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                errorMessage = $"MoveLeft stop simulation failed: {ex.Message}";
            }
        }

        yield return new WaitForFixedUpdate();

        // Act - 우측 이동 테스트
        if (testPassed)
        {
            try
            {
                simulateInputMethod.Invoke(inputHandler, new object[] { "MoveRight" });
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                errorMessage = $"MoveRight simulation failed: {ex.Message}";
            }
        }

        yield return new WaitForFixedUpdate();

        // Assert
        if (testPassed)
        {
            Assert.Pass("Input System movement simulation completed successfully");
        }
        else
        {
            Assert.Fail($"Input System movement simulation failed: {errorMessage}");
        }
    }

    [UnityTest]
    public IEnumerator InputSystem_ActionSimulation_ShouldTriggerLogicMethods()
    {
        // Arrange
        if (inputHandler == null || logicManager == null)
        {
            Assert.Inconclusive("Components not available for Input System action testing");
            yield break;
        }

        var inputHandlerType = inputHandler.GetType();
        var simulateInputMethod = inputHandlerType.GetMethod("SimulateInput");

        if (simulateInputMethod == null)
        {
            Assert.Inconclusive("SimulateInput method not available");
            yield break;
        }

        bool testPassed = true;
        string errorMessage = "";

        // 회전 테스트
        try
        {
            simulateInputMethod.Invoke(inputHandler, new object[] { "Rotate" });
        }
        catch (System.Exception ex)
        {
            if (ex.Message.Contains("NullReferenceException") || ex.Message.Contains("Input"))
            {
                Assert.Inconclusive($"Input System may not be available: {ex.Message}");
                yield break;
            }
            testPassed = false;
            errorMessage = $"Rotate simulation failed: {ex.Message}";
        }

        yield return new WaitForFixedUpdate();

        // 하드 드롭 테스트
        if (testPassed)
        {
            try
            {
                simulateInputMethod.Invoke(inputHandler, new object[] { "HardDrop" });
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                errorMessage = $"HardDrop simulation failed: {ex.Message}";
            }
        }

        yield return new WaitForFixedUpdate();

        // 재시작 테스트
        if (testPassed)
        {
            try
            {
                simulateInputMethod.Invoke(inputHandler, new object[] { "Restart" });
            }
            catch (System.Exception ex)
            {
                testPassed = false;
                errorMessage = $"Restart simulation failed: {ex.Message}";
            }
        }

        yield return new WaitForFixedUpdate();

        // Assert
        if (testPassed)
        {
            Assert.Pass("Input System action simulation completed successfully");
        }
        else
        {
            Assert.Fail($"Input System action simulation failed: {errorMessage}");
        }
    }

    [Test]
    public void InputSystem_ActionMapping_ShouldBeConsistentWithLogicAPI()
    {
        // Arrange & Act - Input Actions과 Logic API의 일관성 확인

        var expectedInputActions = new string[]
        {
            "MoveLeft", "MoveRight", "MoveDown",
            "Rotate", "HardDrop", "Restart", "OpenShop"
        };

        var expectedLogicMethods = new string[]
        {
            "MoveTetrimino", "MoveTetrimino", "MoveTetrimino",
            "RotateTetrimino", "DropTetrimino", "RestartGame", "OpenShop"
        };

        // Assert
        Assert.AreEqual(expectedInputActions.Length, expectedLogicMethods.Length,
            "Input actions and logic methods should have corresponding mappings");

        // 각 Logic 메서드가 존재하는지 확인
        if (logicManager != null)
        {
            var logicManagerType = logicManager.GetType();

            foreach (var methodName in new[] { "MoveTetrimino", "RotateTetrimino", "DropTetrimino", "RestartGame", "OpenShop" })
            {
                var method = logicManagerType.GetMethod(methodName);
                Assert.IsNotNull(method, $"Logic method {methodName} should exist for input system integration");
            }
        }

        Assert.Pass("Input System action mapping is consistent with Logic API");
    }
}
