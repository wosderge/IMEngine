using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InteractiveMusicSystem {
    public class InteractiveMusicEngine : MonoBehaviour {

        class IMTask {
            public float note;
            public int count;
            public IMTask(float _s, int _c) { note = _s; count = _c; }
        };

        static InteractiveMusicEngine self;

        Dictionary<AudioClip, IMTask> taskList;
        AudioSource mysource;
        Dictionary<string, AudioClip> seClips;
        IMSO src;
        float nowBPM;
        float oldclock;
        bool playing;
        float myclock;
        int phase = 0;
        int nextcounter;

        IMPhase NowPhase { get { return src.sequences[phase]; } }

        // 起動
        public static void Run(IMSO src) {
            if (self == null) {
                GameObject g = new GameObject();
                self = g.AddComponent<InteractiveMusicEngine>();
                g.name = "IMEngine";
                self.mysource = g.AddComponent<AudioSource>();


            }
            self.Init(src);
        }

        // 進行セット（多重）
        public static void AddTransitCounter(int stackMax=8) {
            self.nextcounter = Mathf.Min(self.nextcounter + 1, stackMax);
        }

        // 進行セット（単独）
        public static void BeginTransitToNextSequence() {
            self.nextcounter = 1;
        }


        // SE登録されてるやつをそのまま再生する
        public static bool PlayDirect(string key) {
            if (self.seClips.ContainsKey(key) == false) return false;
            self.mysource.PlayOneShot(self.seClips[key]);
            return true;
        }

        // パルス取得
        public static bool IsPluseTiming(float note=4) {
            if (note <= 0) return false;
            float nextclock = self.myclock + Time.deltaTime * self.nowBPM / 60.0f;
            float nowclock = self.myclock;
            if (nextclock >= self.Total()) return true;
            int mycount = 0;
            int nextcount = 0;
            float ones = 4 / note;
            while (nowclock > 0) { mycount++; nowclock -= ones; }
            while (nextclock > 0) { nextcount++; nextclock -= ones; }
            return mycount != nextcount;
        }

        //フェイズ取得
        public static float GetPhase() {
            return self.myclock;
        }

        //フェイズ進行パルス
        public static bool IsTransittingFrame() {
            float nextclock = self.myclock + Time.deltaTime * self.nowBPM / 60.0f;
            if (self.GoNext() &&nextclock >= self.Total() && self.src.sequences[(self.phase + 1) % self.src.sequences.Count].noEffect == false) return true;
            return false;
        }

        //タスクセット
        public static void QuantizedSE(string key, float note=16, int max = 2) {
            if (self.seClips.ContainsKey(key) == false) return;
            AudioClip clip = self.seClips[key];
            self._SetTask(clip, note, max);
        }

        //タスクセット
        public static void QuantizedSE(AudioClip clip, float note = 16, int max = 2) {
            self._SetTask(clip, note, max);
        }
        public void _SetTask(AudioClip key, float note, int max = 2) {
           
            if (!taskList.TryAdd(key, new IMTask(note, 1))) {
                taskList[key].note = note;
                taskList[key].count = Mathf.Min(taskList[key].count + 1, max);
            }
        }


        // 内部初期化処理
        private void Init(IMSO src) {
            this.src = src;
            playing = true;
            phase = 0;
            myclock = -1 / 10000.0f; // 強引だが許せ
            nowBPM = src.BPM;
            seClips = new Dictionary<string, AudioClip>();
            taskList = new Dictionary<AudioClip, IMTask>();
            foreach (var s in src.soundEffects) {
                if (seClips.ContainsKey(s.key)) {
                    Debug.LogWarning("duplicate key : " + s.key);
                    continue;
                }
                seClips.Add(s.key, s.clip);
            }
        }

        // そのフェイズの拍数を取得
        private float Total() {
            if (NowPhase.style == PhaseStyle.Auto) return NowPhase.auto.length;
            if (NowPhase.style == PhaseStyle.Fork) return NowPhase.fork.firstLength + NowPhase.fork.secondLength;
            if (NowPhase.style == PhaseStyle.Series) return NowPhase.series.sets.Sum(t => t.length);
            return 0;
        }

        private bool GoNext() {
            return nextcounter >= 1 || PhaseStyle.Auto == NowPhase.style;
         }

        // BGM処理
        private void BGMWork() {
            float total = Total();

            if (myclock >= total) {　   // phase一周
                oldclock -= total;
                myclock -= total;
                if (GoNext()) {
                    nextcounter--;
                    phase = (phase + 1) % src.sequences.Count;
                    if (NowPhase.shiftBPM != 0) nowBPM = NowPhase.shiftBPM;
                }
            }

            // スタイルごとに音を再生
            switch (NowPhase.style) {
                case PhaseStyle.Auto: {
                        if (oldclock < 0 && myclock >= 0) mysource.PlayOneShot(NowPhase.auto.clip); break;
                    }
                case PhaseStyle.Fork: {
                        if (oldclock < 0 && myclock >= 0) mysource.PlayOneShot(NowPhase.fork.firstClip);
                        if (oldclock < NowPhase.fork.firstLength && myclock > NowPhase.fork.firstLength) {
                            if (nextcounter >= 1) {
                                mysource.PlayOneShot(NowPhase.fork.changeClip);
                            } else {
                                mysource.PlayOneShot(NowPhase.fork.secondClip);
                            }
                        }

                        break;
                    }
                case PhaseStyle.Series: {
                        float nowtotalseq = 0;
                        for (int i = 0; i < NowPhase.series.sets.Count; i++) {
                            if (oldclock < nowtotalseq && myclock > nowtotalseq)
                                mysource.PlayOneShot(NowPhase.series.sets[i].clip);
                            nowtotalseq += NowPhase.series.sets[i].length;
                        }
                        break;
                    }
            }

        }

        // SE処理
        private void SEWork() {
            foreach (var t in taskList) {
                IMTask nt = t.Value;
                if (nt.count <= 0) continue;
                if (IsPluseTiming(nt.note)) {
                    mysource.PlayOneShot(t.Key);
                    nt.count--;
                }
            }
        }

        // 更新
        private void LateUpdate() {
            if (playing == false) return;
            oldclock = myclock;
            myclock += Time.deltaTime * nowBPM / 60.0f;
            BGMWork();
            SEWork();

        }
    }
}