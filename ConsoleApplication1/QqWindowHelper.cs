using System;
using Accessibility;

namespace ConsoleApplication1
{
    public class QqWindowHelper
    {
        IntPtr _QqWindowHandle;
        string _winTitle;
        IAccessible _inputBox;

        public QqWindowHelper(IntPtr windowHandle, String winTitle)
        {
            _QqWindowHandle = windowHandle;
            _winTitle = winTitle;

            GetAccessibleObjects(_QqWindowHandle, out _inputBox);
        }

        /// <summary>
        /// 返回消息框内容
        /// </summary>
        /// <returns></returns>
        public string GetContent()
        {
            string value = (string)_inputBox.get_accValue(Win32.CHILDID_SELF);
            return value;
        }

        private IAccessible[] GetAccessibleChildren(IAccessible paccContainer)
        {
            IAccessible[] rgvarChildren = new IAccessible[paccContainer.accChildCount];
            int pcObtained;
            Win32.AccessibleChildren(paccContainer, 0, paccContainer.accChildCount, rgvarChildren, out pcObtained);
            return rgvarChildren;
        }
        //按层级找到对象
        public IAccessible GetAccessibleChild(IAccessible paccContainer, int[] array)
        {
            if (array.Length > 0)
            {
                IAccessible result = GetAccessibleChildren(paccContainer)[array[0]];

                int[] array_1 = new int[array.Length - 1];
                for (int i = 0; i < array.Length - 1; i++)
                {
                    array_1[i] = array[i + 1];
                }
                return GetAccessibleChild(result, array_1);
            }
            else
            {
                return paccContainer;
            }
        }

        private void GetAccessibleObjects(System.IntPtr imWindowHwnd, out IAccessible inputBox)
        {
            Guid guidCOM = new Guid(0x618736E0, 0x3C3D, 0x11CF, 0x81, 0xC, 0x0, 0xAA, 0x0, 0x38, 0x9B, 0x71);
            Accessibility.IAccessible IACurrent = null;

            Win32.AccessibleObjectFromWindow(imWindowHwnd, (int)Win32.OBJID_CLIENT, ref guidCOM, ref IACurrent);
            inputBox = null;
            if (IACurrent != null)
            {
                IACurrent = (IAccessible)IACurrent.accParent;
               
                inputBox = GetAccessibleChild(IACurrent, new int[] { 3, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 3, 0 });
            }
        }
    }
}
