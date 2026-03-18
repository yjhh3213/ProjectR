using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
//using VehiclePhysics;

public class TacometerChange : MonoBehaviour
{
    [Header("Needle")]
    public Transform needle;

    [Header("VPP Controller")]
    //public VPVehicleController vehicle;

    public Sprite[] TacometerImage;     // 타코미터 이미지
    public Sprite[] NeedleImage;        // 침 이미지

    [Header("Engine Spec (Inspector Values)")]
    public float idleRpm;
    public float maxRpm;
    public float limiterRpm;
    float enginerpm;

    //GearBox Ratio Spec
    float[] GearBox = { 0.0f, 3.76f, 2.269f, 1.645f, 1.187f, 1.0f, 0.843f };
    

    [Header("Angles")]
    public float minAngle = 180f;       // 0부터 시작하는 타코미터 위치
    public float maxAngle = -60f;        // 해당하는 차량의 최대 타코미터 위치

    float rpm = 0f;
    float currentAngle;
    int gear;
    float gearRatio;

    // 반응 속도 설정
    [Header("Smooth Settings")]
    public float accelSmooth;   // 가속
    public float decelSmooth = 0.25f;   // 감속

    [Header("Startup Animation")]
    public float startupDuration = 1.5f;  // 시동 올리는 연출 시간

    // Start is called before the first frame update
    /*void Start()
    {
        idleRpm = 0f;
        maxRpm = vehicle.engine.maxRpm;
        limiterRpm = 8700f;

        accelSmooth = vehicle.engine.maxIdleThrottle;

        currentAngle = minAngle;
        needle.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }

    // Update is called once per frame
    void Update()
    {

        // 1. 실제 차량의 엔진 RPM 가져오기
        // VPP 버전에 따라 단위가 다를 수 있습니다. 보통 mRPM(1/1000 RPM)을 반환하므로 1000으로 나눕니다.
        // 만약 값이 너무 작게 나오면 '/ 1000.0f'를 제거하세요.
        float realRpm = vehicle.data.Get(Channel.Vehicle, VehicleData.EngineRpm) / 1000.0f;

        // 시동 꺼져있을 때 최소한의 떨림 등을 위해 idleRpm 보정 (선택 사항)
        if (realRpm < idleRpm) realRpm = idleRpm;

        // 2. 바늘 부드럽게 움직이기 (보간)
        rpm = Mathf.Lerp(rpm, realRpm, Time.deltaTime * 10f); // 반응 속도 조절

        // 3. 각도 계산 및 적용
        float ratio = Mathf.Clamp01(rpm / maxRpm);
        float targetAngle = Mathf.Lerp(minAngle, maxAngle, ratio);

        needle.localRotation = Quaternion.Euler(0, 0, targetAngle);

        if (Input.GetKeyDown(KeyCode.K))
        {
            idleRpm = vehicle.engine.idleRpm;
        }
        gear = vehicle.data.Get(Channel.Input, InputData.ManualGear);

        //print("gear : " + gear);

        gearRatio = 0f;

        float throttle = vehicle.data.Get(Channel.Input, InputData.Throttle);
        //print("throttle : " + throttle);
        bool braking = vehicle.data.Get(Channel.Input, InputData.Brake) > 0.1f;

        float targetRpm;

        if (gear > 0 && gear <= GearBox.Length)
            gearRatio = GearBox[gear]; //print(gearRatio);

        // 수정된 Update 로직
        if (throttle > 0.1f && !braking)
        {
            // 가속 시: 목표는 Max RPM
            targetRpm = Mathf.Lerp(idleRpm, maxRpm, throttle);
        }
        else
        {
            // [수정됨] 감속 시: 목표는 무조건 Idle RPM이어야 함
            targetRpm = idleRpm;
        }

        // RPM 갱신 로직 (감속 속도 조절)
        if (rpm < targetRpm)
        {
            // 가속 시
            rpm = Mathf.Lerp(rpm, targetRpm, Time.deltaTime * accelSmooth * (gearRatio > 0 ? gearRatio : 1f));
        }
        else
        {
            // [수정됨] 감속 시: 부드럽게 떨어지도록 설정
            // gearRatio가 높을수록(저단기어) 엔진 브레이크가 강하게 걸려 빨리 떨어지는 효과
            float decelSpeed = decelSmooth * (gearRatio > 0 ? gearRatio : 0.5f);
            rpm = Mathf.Lerp(rpm, targetRpm, Time.deltaTime * decelSpeed);
        }
        if (rpm > limiterRpm)
        {
            float cut = Mathf.Sin(Time.time * 80f) * 150f; // Vroom-vroom 효과
            enginerpm = limiterRpm + cut;
            //print("cut : " + cut);
        }
        //print("targetAngle : " + targetAngle);
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 10f);
        //print("currentAngle : " + currentAngle);
        needle.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }*/
}
