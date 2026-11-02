using UnityEngine;

// IPlayerStatus의 기본(Legacy) 구현. 기존 Player 컴포넌트를 감싼다.
// Player 내부가 바뀌면 GameManager가 아니라 이 파일만 고치면 된다.
public sealed class LegacyPlayerStatus : IPlayerStatus
{
    private Player player;

    public bool IsDead
    {
        get
        {
            Player found = ResolvePlayer();
            return found != null && found.IsDead;
        }
    }

    public void ResetForNewRun()
    {
        Player found = ResolvePlayer();
        if (found != null)
        {
            found.ResetForNewRun();
        }
    }

    private Player ResolvePlayer()
    {
        if (player == null)
        {
            player = Object.FindFirstObjectByType<Player>();
        }

        return player;
    }
}
