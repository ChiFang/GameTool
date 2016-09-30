using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Runtime.InteropServices;

using System.Diagnostics;


namespace App1_GAME
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //
        private const bool GLOBAL_HOT_KEY = true;
        Thread Thread_Test;
        public const int DEFAULT_STEP = 100;
        public const int DEFAULT_DELAY = 500;

        private static bool m_AutoClick = false;    // Auto Click status

        const int WH_KEYBOARD = 2;
        const int WH_KEYBOARD_LL = 13;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static int m_HookHandle = 0;    // Hook handle
        private HookProc m_KbdHookProc;            // 鍵盤掛鉤函式指標

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // 設置掛鉤.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
        IntPtr hInstance, int threadId);

        // 將之前設置的掛鉤移除。記得在應用程式結束前呼叫此函式.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // 呼叫下一個掛鉤處理常式（若不這麼做，會令其他掛鉤處理常式失效）.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,
        IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        //

        private void button1_Click(object sender, EventArgs e)
        {
            GetMousePos();
        }

        public void GetMousePos()
        {
            // claer first and show position
            textBox1.Clear();
            textBox2.Clear();
            textBox1.AppendText("x:" + System.Windows.Forms.Cursor.Position.X.ToString());
            textBox2.AppendText("y:" + System.Windows.Forms.Cursor.Position.Y.ToString());

            // use as default position
            // claer first and show position
            textBox5.Clear();
            textBox4.Clear();
            textBox5.AppendText(System.Windows.Forms.Cursor.Position.X.ToString());
            textBox4.AppendText(System.Windows.Forms.Cursor.Position.Y.ToString());

            // SET default step and delay
            textBox6.Clear();
            textBox6.AppendText(DEFAULT_STEP.ToString());
            textBox7.Clear();
            textBox7.AppendText(DEFAULT_DELAY.ToString());
        }

        private void Move_Mouse(int a_lX, int a_lY)
        {
            Cursor.Position = new Point(a_lX, a_lY);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {   // Start auto click
                Hot_Key_Install();
                AutoClick_Switch();
            }

            if (e.KeyCode == Keys.Enter)
            {   // Get Mouse's Position
                GetMousePos();
            }

            if (e.KeyCode == Keys.ControlKey)
            {   // Hot Key Switch
                Hot_Key_Switch();
            }
        }

        private void AutoClick()
        {
            int lx = 0, ly = 0, lcnt = 0;
            int ltmpx = 0, ltmpy = 0, lStep = 0;
            int lTimeGap = 1500;

            lx = int.Parse(textBox5.Text);
            ly = int.Parse(textBox4.Text);
            lStep = int.Parse(textBox6.Text);
            lTimeGap = int.Parse(textBox7.Text);

            while (lcnt < 10)
            {
                ltmpx = lx + lStep;
                ltmpy = ly;
                Move_Mouse(ltmpx, ltmpy);
                Mouse.LeftClick();
                Thread.Sleep(lTimeGap); //Delay

                ltmpx = lx - lStep;
                ltmpy = ly;
                Move_Mouse(ltmpx, ltmpy);
                Mouse.LeftClick();
                Thread.Sleep(lTimeGap); //Delay

                // lcnt++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            AutoClick_Switch();
        }

        private void AutoClick_Switch()
        {
            if (m_AutoClick == false)
            {   
                StartAutoClick();
            }
            else
            {
                StopAutoClick();
            }
        }

        private void StartAutoClick()
        {
            Thread_Test = new Thread(AutoClick);

            button5.Text = "ON";
            m_AutoClick = true;

            Thread_Test.IsBackground = true;
            Thread_Test.Start();
        }

        private void StopAutoClick()
        {
            if (Thread_Test != null && Thread_Test.IsAlive)
            {
                Thread_Test.Abort();
                
                button5.Text = "OFF";
                m_AutoClick = false;
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void button5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
           
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Hot_Key_Switch();
        }

        private void Hot_Key_Switch()
        {
            if (m_HookHandle == 0)
            {
                Hot_Key_Install();
            }
            else
            {
                Hot_Key_Uninstall();
            }
        }

        private void Hot_Key_Uninstall()
        {
            bool ret = UnhookWindowsHookEx(m_HookHandle);
            if (ret == false)
            {
                MessageBox.Show("呼叫 UnhookWindowsHookEx 失敗!");
                return;
            }
            m_HookHandle = 0;
            button2.Text = "Install";
        }

        private void Hot_Key_Install()
        {
            if (GLOBAL_HOT_KEY == true)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    m_KbdHookProc = new HookProc(KeyboardHookProc);
                    m_HookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, m_KbdHookProc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            else
            {
                m_KbdHookProc = new HookProc(KeyboardHookProc);
                m_HookHandle = SetWindowsHookEx(WH_KEYBOARD, m_KbdHookProc, IntPtr.Zero, GetCurrentThreadId());
            }


            if (m_HookHandle == 0)
            {
                MessageBox.Show("呼叫 SetWindowsHookEx 失敗!");
                return;
            }
            button2.Text = "Uninstall";
        }

        public int KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // 當按鍵按下及鬆開時都會觸發此函式，這裡只處理鍵盤按下的情形。

            bool isPressed = (lParam.ToInt32() & 0x80000000) == 0;

            if (nCode < 0 || !isPressed)
            {
                return CallNextHookEx(m_HookHandle, nCode, wParam, lParam);
            }

            // 取得欲攔截之按鍵狀態
            KeyStateInfo ctrlKey = KeyboardInfo.GetKeyState(Keys.ControlKey);
            KeyStateInfo altKey = KeyboardInfo.GetKeyState(Keys.Alt);
            KeyStateInfo shiftKey = KeyboardInfo.GetKeyState(Keys.ShiftKey);
            KeyStateInfo f8Key = KeyboardInfo.GetKeyState(Keys.F8);

            if (ctrlKey.IsPressed)
            {
                System.Diagnostics.Debug.WriteLine("Ctrl Pressed!");
            }
            if (altKey.IsPressed)
            {
                System.Diagnostics.Debug.WriteLine("Alt Pressed!");
                AutoClick_Switch();
                Hot_Key_Uninstall();
            }
            if (shiftKey.IsPressed)
            {
                System.Diagnostics.Debug.WriteLine("Shift Pressed!");
            }
            if (f8Key.IsPressed)
            {
                System.Diagnostics.Debug.WriteLine("F8 Pressed!");
            }

            return CallNextHookEx(m_HookHandle, nCode, wParam, lParam);
        }
    }

    public class KeyboardInfo
    {
        private KeyboardInfo() { }

        [DllImport("user32")]
        private static extern short GetKeyState(int vKey);

        public static KeyStateInfo GetKeyState(Keys key)
        {
            int vkey = (int)key;

            if (key == Keys.Alt)
            {
                vkey = 0x12;    // VK_ALT
            }

            short keyState = GetKeyState(vkey);
            int low = Low(keyState);
            int high = High(keyState);
            bool toggled = (low == 1);
            bool pressed = (high == 1);

            return new KeyStateInfo(key, pressed, toggled);
        }

        private static int High(int keyState)
        {
            if (keyState > 0)
            {
                return keyState >> 0x10;
            }
            else
            {
                return (keyState >> 0x10) & 0x1;
            }

        }

        private static int Low(int keyState)
        {
            return keyState & 0xffff;
        }
    }

    public struct KeyStateInfo
    {
        Keys m_Key;
        bool m_IsPressed;
        bool m_IsToggled;

        public KeyStateInfo(Keys key, bool ispressed, bool istoggled)
        {
            m_Key = key;
            m_IsPressed = ispressed;
            m_IsToggled = istoggled;
        }

        public static KeyStateInfo Default
        {
            get
            {
                return new KeyStateInfo(Keys.None, false, false);
            }
        }

        public Keys Key
        {
            get { return m_Key; }
        }

        public bool IsPressed
        {
            get { return m_IsPressed; }
        }

        public bool IsToggled
        {
            get { return m_IsToggled; }
        }
    }




}
