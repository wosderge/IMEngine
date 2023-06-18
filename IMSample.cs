using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractiveMusicSystem;

// IM �T���v��
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
        // �v���C���[�̈ʒu������ɕ\��
        GUI.Box(new Rect(10, 10, 200, 100),
            "�V�[�P���X�ڍs�t���O�F" + next + "\n" +
             "�V�[�P���X�F" + sequence + "\n" + "\n" +
             "Z:�V�[�P���X�ڍs\nX:�������ςȂ��ŏ��ߓ��ŉ��ǉ�\nC:16���N�I���^�C�Y��SE�Đ�", style);
    }

    // �T���v��
    IEnumerator Start() {
        yield return null;
        InteractiveMusicEngine.Run(sample);
        while (true) {

            // Z�ŃV�[�P���X�ڍs�t���O
            if (Input.GetKeyDown(KeyCode.Z)) {
                InteractiveMusicEngine.BeginTransitToNextSequence();
                next = true;
            }

            // �V�[�P���X�ڍs���̏���
            if (InteractiveMusicEngine.IsTransittingFrame()) {
                InteractiveMusicEngine.PlayDirect("next");
                next = false;
                sequence++;
                nextCircle.transform.localScale = Vector3.one * 3;
            }
            // getnext�ɂ��g�k�T���v��
            nextCircle.transform.localScale = Vector3.Lerp(nextCircle.transform.localScale, Vector3.one, Time.deltaTime * 2.5f);


            // X�������ςȂ��Œǉ��t���[�Y
            if (InteractiveMusicEngine.IsPluseTiming(1) && Input.GetKey(KeyCode.X)) {
                InteractiveMusicEngine.PlayDirect("inbomb");
            }

            // C��16���N�I���^�C�Y��SE�Đ�
            if (Input.GetKeyDown(KeyCode.C)) {
                InteractiveMusicEngine.QuantizedSE("shot", 16, 4);
            }

            // getphase�ɂ��g�k�T���v��
            phaseCircle.transform.localScale = Vector3.one * ((InteractiveMusicEngine.GetPhase() % 2.0f) / 2.0f + 1);

            // getpulse�ɂ��g�k�T���v��
            if (InteractiveMusicEngine.IsPluseTiming(4)) {
                beatCircle.transform.localScale = Vector3.one * 2;
            }
            beatCircle.transform.localScale = Vector3.Lerp(beatCircle.transform.localScale, Vector3.one, Time.deltaTime * 5.0f);



            yield return null;
        }
    }

}
