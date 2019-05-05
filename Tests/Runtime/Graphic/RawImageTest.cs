using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Graphics
{
    public class RawImageTest : IPrebuildSetup
    {
        private const int Width = 32;
        private const int Height = 32;

        // Hook into the graphic callback so we can do our check.
        public class Hook : RawImage
        {
            public bool isGeometryUpdated;
            public bool isCacheUsed;
            public bool isLayoutRebuild;
            public bool isMaterialRebuild;

            public void ResetTest()
            {
                isGeometryUpdated = false;
                isLayoutRebuild = false;
                isMaterialRebuild = false;
                isCacheUsed = false;
            }

            public override void SetLayoutDirty()
            {
                base.SetLayoutDirty();
                isLayoutRebuild = true;
            }

            public override void SetMaterialDirty()
            {
                base.SetMaterialDirty();
                isMaterialRebuild = true;
            }

            protected override void UpdateGeometry()
            {
                base.UpdateGeometry();
                isGeometryUpdated = true;
            }
        }

        private GameObject m_PrefabRoot;
        private Hook m_image;
        private Texture2D m_defaultTexture;

        const string kPrefabPath = "Assets/Resources/RawImageUpdatePrefab.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            var rootGO = new GameObject("Root");
            var canvasGO = new GameObject("Canvas", typeof(Canvas));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGO.transform.SetParent(rootGO.transform);

            var imageGO = new GameObject("Image", typeof(Hook), typeof(RectTransform));
            var imageTransform = imageGO.GetComponent<RectTransform>();
            imageTransform.SetParent(canvas.transform);

            imageTransform.anchoredPosition = Vector2.zero;

            if (!Directory.Exists("Assets/Resources/"))
                Directory.CreateDirectory("Assets/Resources/");

            PrefabUtility.SaveAsPrefabAsset(rootGO, kPrefabPath);
            GameObject.DestroyImmediate(rootGO);
#endif
        }

        [SetUp]
        public void TestSetup()
        {
            m_PrefabRoot = Object.Instantiate(Resources.Load("RawImageUpdatePrefab")) as GameObject;

            m_image = m_PrefabRoot.transform.Find("Canvas/Image").GetComponent<Hook>();
            m_defaultTexture = new Texture2D(Width, Height);
            m_image.texture = m_defaultTexture;
        }

        [UnityTest]
        [Ignore("Fails")]
        public IEnumerator Sprite_Material()
        {
            m_image.ResetTest();

            // can test only on texture change, same texture is bypass by RawImage property
            m_image.texture = new Texture2D(Width, Height);
            yield return new WaitUntil(() => m_image.isGeometryUpdated);

            // validate that layout change rebuil is called
            Assert.IsTrue(m_image.isMaterialRebuild);
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(m_PrefabRoot);
        }
    }
}
