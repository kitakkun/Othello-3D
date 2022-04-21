using UnityEngine;

namespace GameSystem
{
    public class Disc : MonoBehaviour
    {
        private Animator _animator;

        void Start()
        {
            _animator = GetComponent<Animator>();
        }

    }
}