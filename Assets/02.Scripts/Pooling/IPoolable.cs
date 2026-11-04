using UnityEngine;

// 인스턴스가 풀에서 꺼내지고, 반환되는 기능에 관한 인터페이스
public interface IPoolable
{
    public void OnRent();
    public void OnReturn();
}