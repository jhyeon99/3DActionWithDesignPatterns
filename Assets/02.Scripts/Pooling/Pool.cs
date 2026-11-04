using UnityEngine;
using System.Collections.Generic;

public class Pool<T> where T : Component
{
    private readonly T _prefab; // 프리팹 원본
    private readonly Transform _root; // 인스턴스가 생성되는 부모

    private Stack<T> _stack;

    // 생성자 함수 (원본 프리팹, 미리 만들어둘 갯수, 프리팹 생성 위치)를 매개변수로 받는다.
    public Pool(T Prefab, int prewarm, Transform root = null)
    {
        _prefab = Prefab;

        if (_root == null)
        {
            new GameObject($"[Pool] {_prefab.name}");
        }
        else
        {
            _root = root;
        }
    }

    // 프리팹 생성 메서드
    private T CreateInstance()
    {
        T instance = Object.Instantiate(_prefab, _root);

        // TODO : instance 상태 초기화 메서드 호출

        instance.transform.SetParent(_root);
        instance.gameObject.SetActive(false);

        return instance;
    }
}