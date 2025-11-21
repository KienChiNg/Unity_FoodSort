using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using DG.Tweening;
// using MoreMountains.Feedbacks;
// using Spine.Unity;
using UnityEngine;


namespace FoodSort
{
    public class NormalCustomer : MonoBehaviour
    {
        [SerializeField] private Transform _customerMouth;
        [SerializeField] private Animator _animator;
        [SerializeField] private ParticleSystem vfx_smoke_capy;
        [SerializeField] private ParticleSystem vfx_sweat_capy;
        [SerializeField] private ParticleSystem vfx_angry_capy;

        private Queue<Skewer> _skewerQueue = new Queue<Skewer>();
        private bool _isEating = false;

        private LevelManager LevelManager => LevelManager.Instance;
        private SoundManager SoundManager => SoundManager.Instance;
        private OrderManager OrderManager => OrderManager.Instance;

        public async void AddSkewerOnCustomer(Skewer skewer)
        {
            _skewerQueue.Enqueue(skewer);

            if (_isEating) return;

            _isEating = true;
            await ProcessSkewerQueue();
            _isEating = false;
        }

        public void MakeCustomerAnrgy()
        {
            _animator.SetBool("angry", true);
            vfx_angry_capy.Play();
        }

        public void MakeCustomerSad()
        {
            if (_isEating) return;

            _animator.SetTrigger("sad");
            vfx_sweat_capy.Play();
        }

        public void StopCustomerAngry()
        {
            if (_isEating) return;

            _animator.SetBool("angry", false);
            vfx_angry_capy.Stop();
        }

        private async Task ProcessSkewerQueue()
        {
            while (_skewerQueue.Count > 0)
            {
                Skewer skewer = _skewerQueue.Dequeue();
                skewer.transform.parent = _customerMouth;
                _ = skewer.SkewerAnimMoveInCustomer(_customerMouth.position);
                await UniTask.WaitUntil(() => skewer.IsAlmostMovingInCustomer);
                await PlayEatAnimation();
            }
            LevelManager.WinLevel();
        }

        private async Task PlayEatAnimation()
        {
            _animator.SetTrigger("eat");

            await UniTask.WaitUntil(() =>
                !_animator.GetCurrentAnimatorStateInfo(0).IsName("eat")
            );

            SoundManager.PlayCabyHmm();
            vfx_smoke_capy.Play();
        }
    }
}