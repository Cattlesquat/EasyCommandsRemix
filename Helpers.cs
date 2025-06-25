using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCommand
{
    public static class Helpers
    {
        public static char[] GetHotkeys(int length)
        {
            const string optionChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            var hotkeys = new char[length];

            for (int i = 0; i < length; i++)
            {
                if (i < optionChars.Length)
                {
                    hotkeys[i] = optionChars[i];
                }
                else
                {
                    hotkeys[i] = ' ';
                }
            }

            return hotkeys;
        }

        public static char[] GetHotkeys(string[] options)
        {
            return GetHotkeys(options.Length);
        }
    }
}
