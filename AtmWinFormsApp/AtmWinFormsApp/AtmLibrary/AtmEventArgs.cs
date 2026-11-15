using System;

namespace AtmLibrary
{
    public class AtmEventArgs : EventArgs
    {
        public string Message { get; }
        public bool IsSuccess { get; }
        public DateTime Timestamp { get; }

        public AtmEventArgs(string message, bool isSuccess = true)
        {
            Message = message;
            IsSuccess = isSuccess;
            Timestamp = DateTime.Now;
        }
    }

    public delegate void AtmEventHandler(object sender, AtmEventArgs e);
}