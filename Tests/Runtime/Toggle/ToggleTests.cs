using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UI.Tests;
using Object = UnityEngine.Object;

namespace ToggleTest
{
    abstract class BaseToggleTests : IPrebuildSetup
    {
        const string kPrefabTogglePath = "Assets/Resources/TestToggle.prefab";
        const string kPrefabToggleGroupPath = "Assets/Resources/TestToggleGroup.prefab";

        protected GameObject m_PrefabRoot;
        protected List<Toggle> m_toggle = new List<Toggle>();
        protected static int nbToggleInGroup = 2;

        public void Setup()
        {
#if UNITY_EDITOR
            CreateSingleTogglePrefab();
            CreateToggleGroupPrefab();
#endif
        }

        private static void CreateSingleTogglePrefab()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");

            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.referencePixelsPerUnit = 100;

            var toggleGO = new GameObject("TestToggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggleGO.transform.SetParent(canvasGO.transform);

            var toggle = toggleGO.GetComponent<Toggle>();
            toggle.enabled = true;
            toggle.graphic = toggleGO.GetComponent<Image>();
            toggle.graphic.canvasRenderer.SetColor(Color.white);

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabTogglePath);
#endif
        }

        private static void CreateToggleGroupPrefab()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("rootGo");
            GameObject canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            canvasGO.transform.SetParent(rootGO.transform);

            var toggleGroupGO = new GameObject("ToggleGroup", typeof(RectTransform), typeof(ToggleGroup));
            toggleGroupGO.transform.SetParent(canvasGO.transform);
            toggleGroupGO.AddComponent(typeof(ToggleGroup));

            var toggle0GO = new GameObject("TestToggle0", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggle0GO.transform.SetParent(toggleGroupGO.transform);

            var toggle1GO = new GameObject("TestToggle1", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggle1GO.transform.SetParent(toggleGroupGO.transform);

            var toggle = toggle0GO.GetComponent<Toggle>();
            toggle.graphic = toggle0GO.GetComponent<Image>();
            toggle.graphic.canvasRenderer.SetColor(Color.white);

            var toggle1 = toggle1GO.GetComponent<Toggle>();
            toggle1.graphic = toggle1GO.GetComponent<Image>();
            toggle1.graphic.canvasRenderer.SetColor(Color.white);


            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabToggleGroupPath);
#endif
        }

        [SetUp]
        public virtual void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("TestToggle")) as GameObject;
            m_toggle.Add(m_PrefabRoot.GetComponentInChildren<Toggle>());
        }

        [TearDown]
        public virtual void TearDown()
        {
            m_toggle.Clear();
            Object.Destroy(m_PrefabRoot);
        }
    }

    [Ignore("Results in error building player (1139182)")]
    class ToggleTests : BaseToggleTests
    {
        [Test]
        public void SetIsOnWithoutNotifyWillNotNotify()
        {
            m_toggle[0].isOn = false;
            bool calledOnValueChanged = false;
            m_toggle[0].onValueChanged.AddListener(b => { calledOnValueChanged = true; });
            m_toggle[0].SetIsOnWithoutNotify(true);
            Assert.IsTrue(m_toggle[0].isOn);
            Assert.IsFalse(calledOnValueChanged);
        }

        [Test]
        public void NonInteractableCantBeToggled()
        {
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].interactable = false;
            m_toggle[0].OnSubmit(null);
            Assert.IsTrue(m_toggle[0].isOn);
        }

        [Test]
        public void InactiveCantBeToggled()
        {
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].enabled = false;
            m_toggle[0].OnSubmit(null);
            Assert.IsTrue(m_toggle[0].isOn);
        }
    }

    [Ignore("Results in error building player (1139182)")]
    class ToggleGroupTests : BaseToggleTests
    {
        private ToggleGroup m_toggleGroup;

        [SetUp]
        public override void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("TestToggleGroup")) as GameObject;

            m_toggleGroup = m_PrefabRoot.GetComponentInChildren<ToggleGroup>();
            m_toggle.AddRange(m_PrefabRoot.GetComponentsInChildren<Toggle>());
        }

        [TearDown]
        public override void TearDown()
        {
            m_toggleGroup = null;
            m_toggle.Clear();
            Object.Destroy(m_PrefabRoot);
        }

        [Test]
        public void TogglingOneShouldDisableOthersInGroup()
        {
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            m_toggle[1].isOn = true;
            Assert.IsFalse(m_toggle[0].isOn);
            Assert.IsTrue(m_toggle[1].isOn);
        }

        [Test]
        public void DisallowSwitchOffShouldKeepToggleOnWhenClicking()
        {
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Assert.IsTrue(m_toggle[0].isOn);
            Assert.IsFalse(m_toggle[1].isOn);
        }

        [Test]
        public void DisallowSwitchOffShouldDisableToggleWhenClicking()
        {
            m_toggleGroup.allowSwitchOff = true;
            m_toggle[0].group = m_toggleGroup;
            m_toggle[1].group = m_toggleGroup;
            m_toggle[0].isOn = true;
            Assert.IsTrue(m_toggle[0].isOn);
            m_toggle[0].OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Assert.IsFalse(m_toggle[0].isOn);
            Assert.IsFalse(m_toggle[1].isOn);
        }
    }
}
