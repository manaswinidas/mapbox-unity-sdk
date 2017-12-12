namespace Mapbox.Examples
{
	using UnityEngine;
	using UnityEngine.UI;

	public class textDebug : MonoBehaviour
	{

		Text _text;

		void Start()
		{

			_text = GetComponent<Text>();

			//Debug.Log(_text.text);

		}
	}
}