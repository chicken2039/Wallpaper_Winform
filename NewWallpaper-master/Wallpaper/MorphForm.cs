using St2D;
using c = St2D.Components;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Wallpaper.Controls;
using Morphing;

namespace Wallpaper
{
    public partial class MorphForm : Form
    {
        public MorphForm(int ownerScreenIndex = 0)
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            OwnerScreenIndex = ownerScreenIndex;
            Rectangle r = Screen.PrimaryScreen.Bounds;
            ClientSize = r.Size;
            renderer = new Renderer(this);
#if DEBUG
            renderer.AddScene(new c.FPSScene(renderer));
#endif
        }
        #region Variables
        public Renderer renderer;
        private string m_gifpath = "";
        public string ImagePath { get { return m_gifpath; } set { m_gifpath = value; } }
        //################################################//
        private string text_First;
        public string Text_First { get => text_First; set => text_First = value; }
        //################################################//
        private string text_Second;
        public string Text_Second { get => text_Second; set => text_Second = value; }
        //################################################//
        private bool m_isFixed = false;
        public bool IsFixed
        { get { return m_isFixed; } }
        //################################################//
        public WinApi.MONITORINFO OwnerScreen
        {
            get
            {
                if (OwnerScreenIndex < ScreenUtility.Screens.Length)
                    return ScreenUtility.Screens[OwnerScreenIndex];
                return new WinApi.MONITORINFO()
                {
                    rcMonitor = Screen.PrimaryScreen.Bounds,
                    rcWork = Screen.PrimaryScreen.WorkingArea,
                };
            }
        }
        //################################################//
        private int m_ownerScreenIndex = 0;
        public int OwnerScreenIndex
        {
            get { return m_ownerScreenIndex; }
            set
            {
                if (value < 0)
                    value = 0;
                else if (value >= Screen.AllScreens.Length)
                    value = 0;

                if (m_ownerScreenIndex != value)
                {
                    m_ownerScreenIndex = value;

                    PinToBackground();
                }
            }
        }



        //################################################//
        private Task m_checkParent = null;
        private bool m_onRunning = false;
        private EventWaitHandle m_waitHandle = null;
        //################################################//
        private readonly object m_lockFlag = new object();
        private bool m_needUpdate = false;
        #endregion
        #region Pin_Background
        protected bool PinToBackground()
        {
            m_isFixed = BehindDesktopIcon.FixBehindDesktopIcon(Handle);

            if (m_isFixed)
            {
                ScreenUtility.FillScreen(this, OwnerScreen);
            }


            return m_isFixed;
        }

        protected void CheckParent(object thisHandle)
        {
            IntPtr me = (IntPtr)thisHandle;


            while (m_onRunning)
            {
                bool isChildOfProgman = false;
                var progman = WinApi.FindWindow("Progman", null);
                WinApi.EnumChildWindows(progman, new WinApi.EnumWindowsProc((handle, lparam) =>
                {
                    if (handle == me)
                    {
                        isChildOfProgman = true;
                        return false;
                    }
                    return true;
                }), IntPtr.Zero);

                if (isChildOfProgman == false)
                {
                    lock (m_lockFlag)
                    {
                        m_needUpdate = true;
                    }
                }

                m_waitHandle.WaitOne(2000);
            }
        }
        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            lock (m_lockFlag)
            {
                m_needUpdate = true;
            }
        }
        #endregion
        #region Methods
        public void Start()
        {
            var background = new c.Image(renderer);
            background.SetSource(ImagePath);
            renderer.AddScene(background);
            var f = new Font("맑은 고딕",100,FontStyle.Bold);
            var mm = new MorphingManager(renderer);
            mm.AddMorphing(CharacterFactory.ToBitmap(Text_First, f.Clone() as Font), CharacterFactory.ToBitmap(Text_Second, f.Clone() as Font));
            renderer.AddScene(mm);
            mm.Begin();
            renderer.Run();
        }
        #endregion

        private void MorphForm_Load(object sender, EventArgs e)
        {
            PinToBackground();
            Start();
        }
    }
}
