# 🎮 Rogue Tetris Proto

**ECS 기반 테트리스 + 로그라이크 게임 프로토타입**

Unity 2022.3 LTS를 기반으로 제작된 혁신적인 테트리스 게임입니다. 전통적인 OOP 구조에서 **Entity-Component-System (ECS)** 아키텍처로 리팩토링하여 확장성과 성능을 극대화했습니다.

## ✨ 주요 특징

### 🏗️ ECS 아키텍처
- **OVFL.ECS**: 자체 개발한 경량 ECS 프레임워크
- **명령 패턴**: View-Logic 완전 분리를 위한 Message-Command 시스템
- **컴포넌트 기반**: 유연하고 확장 가능한 게임 로직
- **시스템 라이프사이클**: Setup → Tick → Cleanup → Teardown

### 🎯 게임플레이 혁신
- **로그라이크 요소**: 색상/타입 기반 특수 효과 시스템
- **발라트로 스타일**: 조합과 시너지를 통한 전략적 게임플레이
- **동적 보드**: 실시간으로 변화하는 게임 환경
- **확장 가능한 효과**: 새로운 테트리미노 효과 쉽게 추가 가능

### 🔧 기술적 특징
- **Unity Package Manager**: 독립적인 ECS 패키지로 재사용 가능
- **DOTween 통합**: 부드러운 UI 애니메이션
- **모듈러 설계**: 씬별 독립적인 매니저 시스템
- **타입 안전성**: 제네릭 기반 컴파일 타임 검증

## 📦 프로젝트 구조

```
├── Assets/
│   ├── Scripts/
│   │   ├── Logic/              # ECS 로직 레이어
│   │   │   ├── Component/      # 게임 데이터 컴포넌트
│   │   │   ├── System/         # 게임 로직 시스템
│   │   │   └── Command/        # 명령 패턴 구현
│   │   ├── View/               # UI/View 레이어
│   │   │   ├── CanvasManager/  # 씬별 캔버스 관리
│   │   │   └── Interface/      # View 인터페이스
│   │   └── ECSManager.cs       # ECS 총괄 매니저
│   └── Scenes/                 # 게임 씬들
├── Packages/
│   └── com.ovfl.ecs/           # 자체 ECS 프레임워크
│       ├── Runtime/
│       ├── Tests/
│       └── Documentation/
└── README.md
```

## 🚀 시작하기

### 시스템 요구사항
- **Unity**: 2022.3 LTS 이상
- **.NET**: Standard 2.1
- **플랫폼**: Windows, macOS, Linux

### 설치 방법
1. 저장소 클론
```bash
git clone https://github.com/Overflower706/RogueTetrisproto.git
cd RogueTetrisproto
```

2. Unity에서 프로젝트 열기
3. Package Manager에서 OVFL.ECS 패키지 확인
4. Play 버튼으로 게임 실행

## 🎮 게임플레이

### 기본 조작
- **이동**: A/D 또는 ←/→
- **회전**: W 또는 ↑
- **빠른 낙하**: S 또는 ↓
- **즉시 낙하**: Space

### 특수 시스템
- **색상 효과**: 같은 색상 테트리미노 조합 시 보너스
- **타입 효과**: 특정 모양 조합으로 특수 능력 발동
- **콤보 시스템**: 연속 라인 클리어로 점수 배율 증가

## 🏗️ ECS 아키텍처

### 핵심 컴포넌트
```csharp
// 테트리미노 데이터
public class TetriminoComponent : IComponent
{
    public TetriminoType Type;
    public TetriminoShape Shape;
    public int Rotation;
    public Color ColorValue;
}

// 위치 정보
public class PositionComponent : IComponent
{
    public int X, Y;
}

// 게임 상태
public class GameStateComponent : IComponent
{
    public GameState CurrentState;
    public float GameTime;
}
```

### 주요 시스템
- **GameStateSystem**: 게임 상태 및 흐름 관리
- **CommandSystem**: 명령 처리 및 실행
- **TetriminoSpawnSystem**: 테트리미노 생성 및 관리
- **LineClearSystem**: 라인 클리어 로직
- **GravitySystem**: 중력 및 자동 낙하

### 명령 패턴
```csharp
// View에서 로직으로 메시지 전송
var msgEntity = context.CreateEntity();
msgEntity.AddComponent(new MessageComponent 
{ 
    MessageType = MessageType.StartGame,
    Data = gameConfig
});

// 시스템에서 명령 처리
public class GameStateSystem : ITickSystem
{
    public void Tick(Context context)
    {
        var startCommands = context.GetEntitiesWithComponent<StartGameCommand>();
        foreach (var cmd in startCommands)
        {
            StartGame();
            context.DestroyEntity(cmd);
        }
    }
}
```

## 📈 개발 로드맵

### ✅ 완료된 기능
- [x] 기본 ECS 프레임워크 구축
- [x] Message-Command 패턴 구현
- [x] 게임 상태 관리 시스템
- [x] 제네릭 메서드 지원 (`AddSystem<T>()`)
- [x] 시스템 Context 자동 할당

### 🚧 진행 중
- [ ] 테트리미노 스폰 시스템
- [ ] 입력 처리 시스템
- [ ] 라인 클리어 로직
- [ ] View-ECS 통합

### 🎯 향후 계획
- [ ] 로그라이크 효과 시스템
- [ ] 멀티플레이어 지원
- [ ] 레벨 에디터
- [ ] 모드 확장 (배틀, 퍼즐 등)

## 🔧 개발자 가이드

### ECS 시스템 추가하기
```csharp
public class MyCustomSystem : ITickSystem
{
    public Context Context { get; set; }
    
    public void Tick(Context context)
    {
        var entities = context.GetEntitiesWithComponent<MyComponent>();
        foreach (var entity in entities)
        {
            // 로직 처리
        }
    }
}

// ECSManager에 등록
Systems.AddSystem<MyCustomSystem>();
```

### 새로운 컴포넌트 정의
```csharp
public class MyComponent : IComponent
{
    public int Value;
    public string Name;
}
```

### 명령 생성 및 처리
```csharp
// 명령 정의
public class MyCommand : ICommand
{
    public string Parameter;
}

// 메시지 전송
SendMessage(MessageType.Custom, new MyCommand { Parameter = "test" });
```

## 🧪 테스트

프로젝트는 포괄적인 테스트 스위트를 포함합니다:

```bash
# Unity Test Runner에서 실행
- ECS 핵심 기능 테스트
- 통합 테스트
- 성능 테스트
```

## 📚 문서

- [ECS 아키텍처 가이드](./Packages/com.ovfl.ecs/Documentation~/README.md)
- [API 레퍼런스](./Packages/com.ovfl.ecs/Documentation~/API.md)
- [변경 로그](./Packages/com.ovfl.ecs/CHANGELOG.md)
- [개발 TODO](./Packages/com.ovfl.ecs/TODO.md)

## 🤝 기여하기

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 라이선스

이 프로젝트는 MIT 라이선스 하에 배포됩니다. 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.

## 👨‍💻 개발자

**Overflower706** - [@Overflower706](https://github.com/Overflower706)

프로젝트 링크: [https://github.com/Overflower706/RogueTetrisproto](https://github.com/Overflower706/RogueTetrisproto)

## 🙏 감사의 말

- Unity Technologies for the amazing game engine
- DOTween for smooth animations
- 테트리스 오리지널 크리에이터들에게 경의를 표합니다

---

⭐ **이 프로젝트가 도움이 되었다면 스타를 눌러주세요!**
