using System.Reflection;
using System.Collections;
using NUnit.Framework;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace UnityEngine.UI.Tests
{
    [TestFixture]
    class SelectableTests
    {
        private class SelectableTest : Selectable
        {
            public bool isStateNormal { get { return currentSelectionState == SelectionState.Normal; } }
            public bool isStateHighlighted { get { return currentSelectionState == SelectionState.Highlighted; } }
            public bool isStateSelected { get { return currentSelectionState == SelectionState.Selected; } }
            public bool isStatePressed { get { return currentSelectionState == SelectionState.Pressed; } }
            public bool isStateDisabled { get { return currentSelectionState == SelectionState.Disabled; } }
        }

        private SelectableTest selectable;

        private CanvasGroup CreateAndParentGroupTo(string name, GameObject child)
        {
            GameObject canvasRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameObject groupGO = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            groupGO.transform.SetParent(canvasRoot.transform);
            child.transform.SetParent(groupGO.transform);
            return groupGO.GetComponent<CanvasGroup>();
        }

        [SetUp]
        public void TestSetup()
        {
            EventSystem.current = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
            GameObject canvasRoot = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            GameObject SelectableGO = new GameObject("Selectable", typeof(RectTransform), typeof(CanvasRenderer));

            SelectableGO.transform.SetParent(canvasRoot.transform);
            selectable = SelectableGO.AddComponent<SelectableTest>();
            selectable.targetGraphic = selectable.gameObject.AddComponent<ConcreteGraphic>();
        }

        [TearDown]
        public void TearDown()
        {
            EventSystem.current = null;
        }

        #region Selected object

        [Test]
        public void SettingCurrentSelectedSelectableNonInteractableShouldNullifyCurrentSelected()
        {
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            selectable.interactable = false;

            // it should be unselected now that it is not interactable anymore
            Assert.IsNull(EventSystem.current.currentSelectedGameObject);
        }

        [Test]
        public void PointerEnterDownShouldMakeItSelectedGameObject()
        {
            Assert.IsNull(EventSystem.current.currentSelectedGameObject);
            selectable.InvokeOnPointerEnter(new PointerEventData(EventSystem.current));
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current));
            Assert.AreEqual(selectable.gameObject, EventSystem.current.currentSelectedGameObject);
        }

        [Test]
        public void OnSelectShouldSetSelectedState()
        {
            Assert.True(selectable.isStateNormal);
            selectable.OnSelect(new BaseEventData(EventSystem.current));
            Assert.True(selectable.isStateSelected);
        }

        [Test]
        public void OnDeselectShouldUnsetSelectedState()
        {
            Assert.True(selectable.isStateNormal);
            selectable.OnSelect(new BaseEventData(EventSystem.current));
            Assert.True(selectable.isStateSelected);
            selectable.OnDeselect(new BaseEventData(EventSystem.current));
            Assert.True(selectable.isStateNormal);
        }

        #endregion

        #region Interactable

        [Test]
        public void SettingCanvasGroupNotInteractableShouldMakeSelectableNotInteractable()
        {
            // Canvas Group on same object
            var group = selectable.gameObject.AddComponent<CanvasGroup>();
            Assert.IsTrue(selectable.IsInteractable());

            group.interactable = false;
            // actual call happens on the native side, cause by interactable = false
            selectable.InvokeOnCanvasGroupChanged();

            Assert.IsFalse(selectable.IsInteractable());
        }

        [Test]
        public void SettingParentCanvasGroupNotInteractableShouldMakeSelectableNotInteractable()
        {
            var canvasGroup = CreateAndParentGroupTo("CanvasGroup", selectable.gameObject);
            Assert.IsTrue(selectable.IsInteractable());

            canvasGroup.interactable = false;
            // actual call happens on the native side, cause by interactable = false
            selectable.InvokeOnCanvasGroupChanged();

            Assert.IsFalse(selectable.IsInteractable());
        }

        [Test]
        public void SettingParentParentCanvasGroupNotInteractableShouldMakeSelectableNotInteractable()
        {
            var canvasGroup1 = CreateAndParentGroupTo("CanvasGroup1", selectable.gameObject);
            var canvasGroup2 = CreateAndParentGroupTo("CanvasGroup2", canvasGroup1.gameObject);
            Assert.IsTrue(selectable.IsInteractable());

            canvasGroup2.interactable = false;
            // actual call happens on the native side, cause by interactable = false
            selectable.InvokeOnCanvasGroupChanged();

            Assert.IsFalse(selectable.IsInteractable());
        }

        [Test]
        public void SettingParentParentCanvasGroupInteractableShouldMakeSelectableInteractable()
        {
            var canvasGroup1 = CreateAndParentGroupTo("CanvasGroup1", selectable.gameObject);
            CreateAndParentGroupTo("CanvasGroup2", canvasGroup1.gameObject);
            Assert.IsTrue(selectable.IsInteractable());

            // actual call happens on the native side, cause by interactable
            selectable.InvokeOnCanvasGroupChanged();

            Assert.IsTrue(selectable.IsInteractable());
        }

        [Test]
        public void SettingParentParentCanvasGroupNotInteractableShouldNotMakeSelectableNotInteractableIfIgnoreParentGroups()
        {
            var canvasGroup1 = CreateAndParentGroupTo("CanvasGroup1", selectable.gameObject);
            canvasGroup1.ignoreParentGroups = true;
            var canvasGroup2 = CreateAndParentGroupTo("CanvasGroup2", canvasGroup1.gameObject);
            Assert.IsTrue(selectable.IsInteractable());

            canvasGroup2.interactable = false;
            // actual call happens on the native side, cause by interactable = false
            selectable.InvokeOnCanvasGroupChanged();

            Assert.IsTrue(selectable.IsInteractable());
        }

        [Test]// regression test 861736
        public void PointerEnterThenSetNotInteractableThenExitThenSetInteractableShouldSetStateToDefault()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            Assert.True(selectable.isStateHighlighted);
            selectable.interactable = false;
            selectable.InvokeOnPointerExit(null);
            selectable.interactable = true;
            Assert.False(selectable.isStateHighlighted);
            Assert.True(selectable.isStateNormal);
        }

        [Test]// regression test 861736
        public void PointerEnterThenSetNotInteractableThenSetInteractableShouldStayHighlighted()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            Assert.True(selectable.isStateHighlighted);
            selectable.interactable = false;
            selectable.interactable = true;
            Assert.True(selectable.isStateHighlighted);
        }

        #endregion

        #region Tweening

        [UnityTest]
        public IEnumerator SettingNotInteractableShouldTweenToDisabledColor()
        {
            var canvasRenderer = selectable.gameObject.GetComponent<CanvasRenderer>();
            selectable.InvokeOnEnable();
            canvasRenderer.SetColor(selectable.colors.normalColor);

            selectable.interactable = false;

            yield return new WaitForSeconds(1);

            Assert.AreEqual(selectable.colors.disabledColor, canvasRenderer.GetColor());

            selectable.interactable = true;

            yield return new WaitForSeconds(1);

            Assert.AreEqual(selectable.colors.normalColor, canvasRenderer.GetColor());
        }

        [UnityTest][Ignore("Fails")] // regression test 742140
        public IEnumerator SettingNotInteractableThenInteractableShouldNotTweenToDisabledColor()
        {
            var canvasRenderer = selectable.gameObject.GetComponent<CanvasRenderer>();
            selectable.enabled = false;
            selectable.enabled = true;
            canvasRenderer.SetColor(selectable.colors.normalColor);

            selectable.interactable = false;
            selectable.interactable = true;
            Color c = canvasRenderer.GetColor();

            for (int i = 0; i < 30; i++)
            {
                yield return null;
                Color c2 = canvasRenderer.GetColor();
                Assert.AreNotEqual(c2, c);
            }
            Assert.AreEqual(selectable.colors.normalColor, canvasRenderer.GetColor());
        }

        [UnityTest]
        public IEnumerator SettingInteractableToFalseTrueFalseShouldTweenToDisabledColor()
        {
            var canvasRenderer = selectable.gameObject.GetComponent<CanvasRenderer>();
            selectable.InvokeOnEnable();
            canvasRenderer.SetColor(selectable.colors.normalColor);

            selectable.interactable = false;
            selectable.interactable = true;
            selectable.interactable = false;

            yield return new WaitForSeconds(1);

            Assert.AreEqual(selectable.colors.disabledColor, canvasRenderer.GetColor());
        }

        [Test]
        public void TriggerAnimationWithNoAnimator()
        {
            Assert.Null(selectable.animator);
            Assert.DoesNotThrow(() => selectable.InvokeTriggerAnimation("asdasd"));
        }

        [Test]
        public void TriggerAnimationWithDisabledAnimator()
        {
            var an = selectable.gameObject.AddComponent<Animator>();
            an.enabled = false;
            Assert.NotNull(selectable.animator);
            Assert.DoesNotThrow(() => selectable.InvokeTriggerAnimation("asdasd"));
        }

        [Test]
        public void TriggerAnimationAnimatorWithNoRuntimeController()
        {
            var an = selectable.gameObject.AddComponent<Animator>();
            an.runtimeAnimatorController = null;
            Assert.NotNull(selectable.animator);
            Assert.DoesNotThrow(() => selectable.InvokeTriggerAnimation("asdasd"));
        }

        #endregion

        #region Selection state and pointer

        [Test]
        public void SelectShouldSetSelectedObject()
        {
            Assert.Null(EventSystem.current.currentSelectedGameObject);
            selectable.Select();
            Assert.AreEqual(selectable.gameObject, EventSystem.current.currentSelectedGameObject);
        }

        [Test]
        public void SelectWhenAlreadySelectingShouldNotSetSelectedObject()
        {
            Assert.Null(EventSystem.current.currentSelectedGameObject);
            var fieldInfo = typeof(EventSystem).GetField("m_SelectionGuard", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldInfo
                .SetValue(EventSystem.current, true);
            selectable.Select();
            Assert.Null(EventSystem.current.currentSelectedGameObject);
        }

        [Test]
        public void PointerEnterShouldHighlight()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            Assert.True(selectable.isStateHighlighted);
        }

        [Test]
        public void PointerEnterAndRightClickShouldHighlightNotPress()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Right
            });
            Assert.True(selectable.isStateHighlighted);
        }

        [Test]
        public void PointerEnterAndRightClickShouldPress()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current));
            Assert.True(selectable.isStatePressed);
        }

        [Test]
        public void PointerEnterLeftClickExitShouldPress()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current));
            selectable.InvokeOnPointerExit(null);
            Assert.True(selectable.isStatePressed);
        }

        [Test]
        public void PointerEnterLeftClickExitReleaseShouldSelect()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current));
            selectable.InvokeOnPointerExit(null);
            selectable.InvokeOnPointerUp(new PointerEventData(EventSystem.current));
            Assert.True(selectable.isStateSelected);
        }

        [Test]
        public void PointerDownShouldSetSelectedObject()
        {
            Assert.Null(EventSystem.current.currentSelectedGameObject);
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current));
            Assert.AreEqual(selectable.gameObject, EventSystem.current.currentSelectedGameObject);
        }

        [Test]
        public void PointerLeftDownRightDownRightUpShouldNotChangeState()
        {
            Assert.True(selectable.isStateNormal);
            selectable.InvokeOnPointerEnter(null);
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            selectable.InvokeOnPointerDown(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
            selectable.InvokeOnPointerUp(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Right });
            Assert.True(selectable.isStatePressed);
        }

        [Test, Ignore("No disabled state assigned ? Investigate")]
        public void SettingNotInteractableShouldDisable()
        {
            Assert.True(selectable.isStateNormal);
            selectable.interactable = false;
            selectable.InvokeOnCanvasGroupChanged();
            Assert.True(selectable.isStateDisabled);
        }

        #endregion

        #region No event system

        [Test] // regression test 787563
        public void SettingInteractableWithNoEventSystemShouldNotCrash()
        {
            EventSystem.current = null;
            selectable.interactable = false;
        }

        [Test] // regression test 787563
        public void OnPointerDownWithNoEventSystemShouldNotCrash()
        {
            EventSystem.current = null;
            selectable.OnPointerDown(new PointerEventData(EventSystem.current) {button = PointerEventData.InputButton.Left});
        }

        [Test] // regression test 787563
        public void SelectWithNoEventSystemShouldNotCrash()
        {
            EventSystem.current = null;
            selectable.Select();
        }

        #endregion
    }
}
