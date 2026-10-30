// 게임 시간 계약. Time.time을 직접 읽지 않고 이 계약을 통해 읽으면, 테스트에서 시간을 마음대로 흘려보낼 수 있다.
public interface IClock
{
    // 게임이 시작된 뒤 지난 시간(초)
    float Time { get; }

    // 지난 프레임에서 지금까지 걸린 시간(초)
    float DeltaTime { get; }
}
