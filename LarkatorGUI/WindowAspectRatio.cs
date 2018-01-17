using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace LarkatorGUI
{
    /// <summary>
    /// Thanks to Mike O'Brien in http://www.mikeobrien.net/blog/maintaining-aspect-ratio-when-resizing for the basis for this.
    /// </summary>
    public class WindowAspectRatio
    {
        private Func<int, int> calculateWidthFromHeightFn;

        private WindowAspectRatio(Window window, Func<int,int> calculateWidthFromHeight)
        {
            this.calculateWidthFromHeightFn = calculateWidthFromHeight;
            ((HwndSource)HwndSource.FromVisual(window)).AddHook(DragHook);
        }

        public static void Register(Window window, Func<int,int> calculateWidthFromHeight)
        {
            new WindowAspectRatio(window, calculateWidthFromHeight);
        }

        internal enum WM
        {
            WINDOWPOSCHANGING = 0x0046,
        }

        [Flags()]
        public enum SWP
        {
            NoMove = 0x2,
        }

        [StructLayout(LayoutKind.Sequential)]
        [System.Diagnostics.DebuggerDisplay("X={x}, Y={y}, CX={cx}, CY={cy}")]
        internal struct WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public int flags;
        }

        private IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        {
            if ((WM)msg == WM.WINDOWPOSCHANGING)
            {
                WINDOWPOS position = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

                if ((position.flags & (int)SWP.NoMove) != 0 ||
                    HwndSource.FromHwnd(hwnd).RootVisual == null) return IntPtr.Zero;

                position.cx = calculateWidthFromHeightFn(position.cy);

                Marshal.StructureToPtr(position, lParam, true);
                handeled = true;
            }

            return IntPtr.Zero;
        }

    }
}