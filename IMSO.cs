using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InteractiveMusicSystem {
	public enum PhaseStyle {
		Fork,
		Series,
		Auto,
	}


	[System.Serializable]
	public class IMForkPhase {
		public float firstLength;
		public float secondLength;
		public AudioClip firstClip;
		public AudioClip secondClip;
		public AudioClip changeClip;
	}

	[System.Serializable]
	public class IMSClip {
		public float length;
		public AudioClip clip;
	}

	[System.Serializable]
	public class IMSeriesPhase {
		public List<IMSClip> sets;
	}
	[System.Serializable]
	public class IMPhase {

		public PhaseStyle style;
		public IMForkPhase fork;
		public IMSeriesPhase series;
		public IMSClip auto;
		public bool noEffect;
		public float shiftBPM;
	}
	[System.Serializable]
	public class IMSE {
		public string key;
		public AudioClip clip;
	}


	[System.Serializable]
	public class IMSO : ScriptableObject {

		public string songName;
		public float BPM = 120;
		public List<IMPhase> sequences;
		public List<IMSE> soundEffects;
	}
}