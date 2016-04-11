using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Audio;

[Serializable]
public class AudioFx {

	[Serializable]
	public class ExposedParameters {
		public string lowpassCutoffFreq = "LowpassCutoffFreq";
	}

	[SerializeField] AudioMixer mixer;
	[SerializeField] float atmosphereLerpRate = 1;
	[SerializeField] float lowpassCutoffMax = 22000, lowpassCutoffMin = 100;
	[SerializeField] ExposedParameters exposedParamNames;
	float atmo;

	
	public void UpdateFx (float timeStep, float atmoLocal) 
	{
		atmo = Mathf.MoveTowards(atmo, atmoLocal, timeStep * atmosphereLerpRate);
		float freq = Mathf.Clamp(atmo, 0, 1) * (lowpassCutoffMax - lowpassCutoffMin) + lowpassCutoffMin;
		mixer.SetFloat(exposedParamNames.lowpassCutoffFreq, freq);
	}

}
