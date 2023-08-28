using UnityEngine;

namespace TarodevController
{


    public interface IMonsterController
    {
        public Vector3 Velocity { get; }
        public FrameInput Input { get; }
        public bool Damaged { get; }
    }
    /// <summary>
    /// This is a pretty filthy script. I was just arbitrarily adding to it as I went.
    /// You won't find any programming prowess here.
    /// This is a supplementary script to help with effects and animation. Basically a juice factory.
    /// </summary>
    /// 



    public class MonsterAnimator : MonoBehaviour
    {

        [field: SerializeField] public Animator _anim { get; private set; }
        [SerializeField] private AudioSource _source;

        private IMonsterController _monster;

        void Awake() => _monster = GetComponent<IMonsterController>();

        void Update()
        {

            if (_monster == null) return;

            // Flip the sprite
            if (_monster.Input.X != 0)
            {
                transform.localScale = new Vector3(_monster.Input.X > 0 ? 1 : -1, 1, 1);

            }

            ChangeAnim();
        }


        // 함수 추가
        private void ChangeAnim()
        {
            // Hurt TODO : PlayerContorller 에서 Dash 하면 이동시키는거 추가하기
            if (_monster.Damaged)
            {
                print("Damaged");
                _anim.SetTrigger(HurtKey);

            }

        }

        #region Animation Keys
        private static readonly int HurtKey = Animator.StringToHash("Hurt");
        private static readonly int DeathKey = Animator.StringToHash("isDeath");

        #endregion
    }
}