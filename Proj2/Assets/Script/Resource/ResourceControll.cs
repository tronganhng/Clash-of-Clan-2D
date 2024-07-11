namespace Proj2.clashofclan_2d
{
    using UnityEngine;
    using UnityEngine.UI;

    public class ResourceControll : MonoBehaviour
    {
        public Text woodtxt, goldtxt, meattxt;
        public int wood_cnt = 0, gold_cnt = 0, meat_cnt = 0;

        // biến tĩnh duy nhất chạy xuyên suốt
        public static ResourceControll _instance = null; 
        // cho phép bên ngoài thay đổi các giá trị của _instance
        public static ResourceControll instance { get { return _instance; } }

        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            SetAllItemCount();
        }

        public void SetAllItemCount()
        {
             goldtxt.text = "" + gold_cnt;
             woodtxt.text = "" + wood_cnt;
             meattxt.text = "" + meat_cnt;
        }

        public void SetItemCount(int index)
        {
            if (index == 0) goldtxt.text = "" + gold_cnt;
            if (index == 1) woodtxt.text = "" + wood_cnt;
            if (index == 2) meattxt.text = "" + meat_cnt;
        }

        public void UpdateItemCnt(int index, int quantity)
        {
            if (index == 0) gold_cnt += quantity;
            if (index == 1) wood_cnt += quantity;
            if (index == 2) meat_cnt += quantity;
        }
    }
}
