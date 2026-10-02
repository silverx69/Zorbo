using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zorbo.Core
{
    public interface IHashlink
    {
        byte[] ToByteArray();
        void FromByteArray(byte[] value);

        string ToPlainText();
        void FromPlainText(string text);
    }
}
