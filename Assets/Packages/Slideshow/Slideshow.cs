using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SlideshowSystem {

	[RequireComponent(typeof(Renderer))]
	public class Slideshow : MonoBehaviour {
		public static readonly string[] IMAGE_EXTS = new string[]{ ".png", ".jpg" };
		public const string CONFIG_FILE = "Config.xml";
		public const string PROP_MAIN_TEX = "_MainTex";
		public const string PROP_FADER = "_Fader";

		public Data data;

		Renderer _rnd;
		Texture[] _images;
		MaterialPropertyBlock _block;

		void Start () {
			_rnd = GetComponent<Renderer>();
			_rnd.GetPropertyBlock(_block = new MaterialPropertyBlock());
			_rnd.SetPropertyBlock(_block);

			if (Application.isEditor)
				SaveConfig();
			LoadConfig();
			LoadImages();

			StartCoroutine(Slider());
		}
		void OnDestroy() {
			foreach (var tex in _images)
				Destroy(tex);
		}
		IEnumerator Slider() {
			while (true) {
				var t = Time.timeSinceLevelLoad;
				var normalizedTime = t / data.duration;
				var i = Mathf.FloorToInt(normalizedTime);
				var normalizedOffset = Mathf.Clamp(normalizedTime - i, 0f, 1f);
				i %= _images.Length;

				_block.SetTexture(PROP_MAIN_TEX, _images[i]);

				if (normalizedOffset < data.fadeIn)
					_block.SetFloat(PROP_FADER, normalizedOffset / data.fadeIn);
				else if (normalizedOffset < (1f - data.fadeOut))
					_block.SetFloat(PROP_FADER, 1f);
				else
					_block.SetFloat(PROP_FADER, (1f - normalizedOffset) / data.fadeOut);

				_rnd.SetPropertyBlock(_block);
				yield return null;
			}
		}

		void LoadConfig () {
			var fullConfigPath = Path.Combine (Application.streamingAssetsPath, CONFIG_FILE);
			if (File.Exists (fullConfigPath))
				using (var reader = new StreamReader(fullConfigPath, new System.Text.UTF8Encoding(false)))
					data = (Data)new XmlSerializer (typeof(Data)).Deserialize(reader);
		}
		void SaveConfig () {
			var fullConfigPath = Path.Combine (Application.streamingAssetsPath, CONFIG_FILE);
			using (var writer = new StreamWriter(File.OpenWrite(fullConfigPath), new System.Text.UTF8Encoding(false)))
				new XmlSerializer(typeof(Data)).Serialize(writer, data);
		}
		void LoadImages () {
			var textures = new List<Texture> ();
			foreach (var filename in Directory.GetFiles (data.imageFolder)) {
				var matchExt = false;
				foreach (var ext in IMAGE_EXTS) {
					if (filename.LastIndexOf (ext) == (filename.Length - ext.Length)) {
						matchExt = true;
						break;
					}
				}
				if (matchExt) {
					var tex = new Texture2D (1, 1, TextureFormat.DXT1, false);
					tex.LoadImage (File.ReadAllBytes (filename));
					tex.filterMode = FilterMode.Bilinear;
					tex.wrapMode = TextureWrapMode.Clamp;
					textures.Add (tex);
				}
			}
			_images = textures.ToArray ();
		}

		[System.Serializable]
		public class Data {
			public string imageFolder;
			public float duration = 10f;
			public float fadeIn = 0.01f;
			public float fadeOut = 0.01f;
		}
	}
}