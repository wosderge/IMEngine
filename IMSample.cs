using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractiveMusicSystem;

// IM サンプル
public class IMSample : MonoBehaviour {

    [SerializeField] IMSO sample;

    [SerializeField] GameObject phaseCircle;
    [SerializeField] GameObject beatCircle;
    [SerializeField] GameObject nextCircle;

    bool next;
    int sequence;
    GUIStyle style = new GUIStyle();

    void OnGUI() {
        style.alignment = TextAnchor.UpperLeft;
        // プレイヤーの位置を左上に表示
        GUI.Box(new Rect(10, 10, 200, 100),
            "シーケンス移行フラグ：" + next + "\n" +
             "シーケンス：" + sequence + "\n" + "\n" +
             "Z:シーケンス移行\nX:押しっぱなしで小節頭で音追加\nC:16分クオンタイズでSE再生", style);
    }

    // サンプル
    IEnumerator Start() {
        yield return null;
        InteractiveMusicEngine.Run(sample);
        while (true) {

            // Zでシーケンス移行フラグ
            if (Input.GetKeyDown(KeyCode.Z)) {
                InteractiveMusicEngine.BeginTransitToNextSequence();
                next = true;
            }

            // シーケンス移行時の処理
            if (InteractiveMusicEngine.IsTransittingFrame()) {
                InteractiveMusicEngine.PlayDirect("next");
                next = false;
                sequence++;
                nextCircle.transform.localScale = Vector3.one * 3;
            }
            // getnextによる拡縮サンプル
            nextCircle.transform.localScale = Vector3.Lerp(nextCircle.transform.localScale, Vector3.one, Time.deltaTime * 2.5f);


            // X押しっぱなしで追加フレーズ
            if (InteractiveMusicEngine.IsPluseTiming(1) && Input.GetKey(KeyCode.X)) {
                InteractiveMusicEngine.PlayDirect("inbomb");
            }

            // Cで16分クオンタイズでSE再生
            if (Input.GetKeyDown(KeyCode.C)) {
                InteractiveMusicEngine.QuantizedSE("shot", 16, 4);
            }

            // getphaseによる拡縮サンプル
            phaseCircle.transform.localScale = Vector3.one * ((InteractiveMusicEngine.GetPhase() % 2.0f) / 2.0f + 1);

            // getpulseによる拡縮サンプル
            if (InteractiveMusicEngine.IsPluseTiming(4)) {
                beatCircle.transform.localScale = Vector3.one * 2;
            }
            beatCircle.transform.localScale = Vector3.Lerp(beatCircle.transform.localScale, Vector3.one, Time.deltaTime * 5.0f);



            yield return null;
        }
    }

}
