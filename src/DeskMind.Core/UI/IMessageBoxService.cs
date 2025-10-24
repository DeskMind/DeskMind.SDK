using System;
using System.Collections.Generic;
using System.Text;

namespace DeskMind.Core.UI
{
    public enum MessageBoxType
    {
        Info,
        Warning,
        Error
    }

    public interface IMessageBoxService
    {
        void Show(string message, string title = "Info");

        bool Confirm(string message, string title = "Confirm");
    }
}

