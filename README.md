# Vibrotactile & force feedback-based tremor alleviation rehabilitation training system
RubberPrintmaking

![image](https://github.com/JWdori/VR-MCT/assets/42615916/0ac09929-e5a0-45df-ba41-61f9e5e410d2)

본 프로젝트는 수전증(손 떨림)을 감지하여, 수전증 증상을 억제하고 재활훈련에 사용할 수 있는 VR 응용시스템을 제시한다. 

개발 게임 엔진은 **Unity 2022**을 사용하였고, 가상현실 HMD는 **Oculus quest 3**, 힘 피드백 햅틱 장치는 **Inverse3**를 사용하였다.

##### 본 프로젝트의 Build 및 실행을 위해서는 haply사의 Inverse3 제품이 필수적으로 필요하다.
<br/>

### :orange_book: Contents
0. [Folder Structure](#folder-structure)
1. [Environment Setting](#environment-setting)
2. [SDK setting Inverse3/OpenXR](#sdk-setting-inverse3)
3. [user manual](#user-manual)
4. [System Modular AI](#system-modular-ai)
5. [Troubleshooting](#Troubleshooting)
6. [ETC](#etc)

<br/>

-----

### Folder Structure

```
📦RubberPrintmaking       
 ┣ 📂Assets             //유니티 코드
 ┃ ┣ 📂 Mainfolder        
 ┃ ┃ ┣ 📂 Editor       // 버텍스 메쉬
 ┃ ┃ ┣ 📂 CSV          // 데이터 저장 csv 파일 경로
 ┃ ┃ ┣ 📂 Force Dat     // 수집한 csv 데이터
 ┃ ┃ ┣ 📂 Physics material      // 고무판 물리 머터리얼
 ┃ ┃ ┣ 📂 Shaders        // 셰이더 코드
 ┃ ┃ ┣ 📂 Scenes         // 메인 씬 및 테스트 씬
 ┃ ┃ ┗ 📂 Scripts        // 소스코드 전체
 ┗ 📂AI              // 손떨림 감지 및 진동 코드
 ┃ ┣ MPU9250_raw       // MPU250 raw 데이터
 ┃ ┣ tranins           // 트레인 데이터셋
 ┃ ┗ .py               // 모델 실행 파일
 ┗ 📂.gitignore      // non_free폴더(유료에셋)는 커밋 제외
```

<br/>
--------------------------------------------------------------------------------------------------------------------------------------------------------------

### Environment Setting

- 구현 환경
  - OS : Window 11 Pro 64 bit
  - CPU : 12th Gen Intel(R) Core(TM) i9-12900K
  - GPU : (NVIDIA GeForce RTX 4070)
  - Unity version : 2022.3.22f1
  - Hardware API for Unity Version 📘 1.1.6-preview                          

- 빌드 환경
  - HMD: Oculus(Meta) quest 3
  - Cable: 링크 케이블                                     //저가 케이블 사용시, 빌드에 문제가 있을 수 있음

    
<br/>
--------------------------------------------------------------------------------------------------------------------------------------------------------------

### SDK setting Inverse3
- Inverse3 기기와 이에 대한 기반 지식이 없다면 사용이 불가능하다. [개발자 지원 페이지]([https://www.bhaptics.com/support/developers](https://develop.haply.co/releases/inverse-sdk-unity))를 참고
- OpenXR은 유니티에서 제공되는 기본 xr플러그인으로, 본 프로젝트에 포함되어 있다.
  - [유니티 VR 튜토리얼](https://learn.unity.com/course/create-with-vr) - 초반 개발자 세팅
  
 
<br/>
--------------------------------------------------------------------------------------------------------------------------------------------------------------

### User manual
Hardware API for Unity Version 📘 1.1.6-preview <--             
플러그인 설치가 완료 되었다면, 프로젝트 실행이 가능하다. 그러나, 원할한 힘 피드백과 고무판화 시뮬레이션을 위해서는 인스펙터 창에서 컴포넌트가 제대로 할당되었나 확인할 필요가 있다.
아래와 같이 할당되어 있다면 문제없이 실행 가능하다. 또한 advanced physics haptic effector의 force feedback type을 수정하여 훈련 시나리오(기본-유도-방해)를 수정할 수 있다.

![image](https://github.com/MILab-SensorimotorSkills/RubberPrintmaking/assets/42615916/df608900-a6ae-4d11-857d-dad223e26b94)
![image](https://github.com/MILab-SensorimotorSkills/RubberPrintmaking/assets/42615916/d897c274-917a-454c-87c4-71e9e486592b)
![image](https://github.com/MILab-SensorimotorSkills/RubberPrintmaking/assets/42615916/5a1eda36-b406-4552-9fb6-b9e7f79b3e43)

[손떨림 AI모델](#system-modular-ai) 를 먼저 실행 한 후,

유니티를 실행하고, space bar와 2key를 차례로 누르면, 고무판화 시뮬레이션 실행이 가능하다.
**메쉬는 아래로 변형 될 것이고, 개별로 버추얼 콜라이더가 움직여 파지는 느낌을 느낄 수 있다**

![image](https://github.com/MILab-SensorimotorSkills/RubberPrintmaking/assets/42615916/d849e281-f165-4854-9a4d-f9d57ca2ec0b)

<br/>
--------------------------------------------------------------------------------------------------------------------------------------------------------------

### [System Modular AI](https://github.com/MILab-SensorimotorSkills/RubberPrintmaking/tree/main/AI) <-- link

<br/>
--------------------------------------------------------------------------------------------------------------------------------------------------------------

### Troubleshooting
   1) inverse3의 팅김 및 연결 오류 --> usb 포트 및 하드웨어와 유니티의 간혈적 충돌 문제
   2) 포스 피드백의 부자연스러움 & 떨림 --> 유니티 엔진의 최적화 한계

<br/>
--------------------------------------------------------------------------------------------------------------------------------------------------------------

### ETC
 - **본 프로젝트는 한양대학교 CAD를위한모델링및시뮬레이션 과제입니다.
