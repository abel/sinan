using System;
using System.Drawing;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// Ŀ�ĵ�..
    /// </summary>
    public class Destination
    {
        public Destination(string sceneID, int x, int y)
        {
            this.SceneID = sceneID;
            this.X = x;
            this.Y = y;
        }

        public string SceneID
        { get; private set; }

        /// <summary>
        /// �������������λ��
        /// </summary>
        public int X
        {
            private set;
            get;
        }

        public int Y
        {
            private set;
            get;
        }
    }

    /// <summary>
    /// �����еĴ�����..
    /// </summary>
    [Serializable]
    public class ScenePin
    {
        Rectangle m_range;
        public Rectangle Range
        {
            get { return m_range; }
        }

        Destination m_destination;
        public ScenePin(Variant gc)
        {
            _id = gc.GetStringOrDefault("_id");
            _autoPath = (gc.GetStringOrDefault("SubType") == "4");
            Variant v = gc.GetVariantOrDefault("Value");
            if (v != null)
            {
                _sceneA = v.GetStringOrDefault("SceneID");
                _x = v.GetIntOrDefault("X");
                _y = v.GetIntOrDefault("Y");
                m_destination = new Destination(
                    v.GetStringOrDefault("SceneB"),
                    v.GetIntOrDefault("BX"),
                    v.GetIntOrDefault("BY")
                    );
                const int s = 4;
                m_range = new Rectangle(_x - s, _y - s, 2 * s, 2 * s);
            }
        }

        #region Entity
        private int _x;
        private int _y;
        private bool _autoPath;
        private string _id;
        private string _sceneA;

        /// <summary>
        /// ������ID
        /// </summary>
        public string ID
        {
            get { return _id; }
        }

        /// <summary>
        /// ������������ڳ���ID
        /// </summary>
        public string SceneA
        {
            get { return _sceneA; }
        }

        /// <summary>
        /// �������������λ��
        /// </summary>
        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        /// <summary>
        /// �����Զ�Ѱ·(4:������(����)��8������(������))
        /// </summary>
        public bool AutoPath
        {
            get { return _autoPath; }
        }

        /// <summary>
        /// Ŀ�ĵ�
        /// </summary>
        public Destination Destination
        {
            get { return m_destination; }
        }

        /// <summary>
        /// �������յ����ڳ���ID
        /// </summary>
        public string SceneB
        {
            get { return m_destination.SceneID; }
        }

        /// <summary>
        /// �յ��X����
        /// </summary>
        public int ScenebX
        {
            get { return m_destination.X; }
        }

        /// <summary>
        /// �յ��Y����
        /// </summary>
        public int ScenebY
        {
            get { return m_destination.Y; }
        }
        #endregion Entity
    }
}

