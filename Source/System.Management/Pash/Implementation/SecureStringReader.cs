using System;
using System.Security;
using System.Threading;

namespace System.Management.Automation
{
    /*
     * Very simple class to read a secure string (like a password) from console. It uses a SecureString object
     * as a buffer, prints only asterisks and supports backspace.
     */
    internal class SecureStringReader
    {
        private Thread _mainThread;

        public SecureString ReadLine()
        {
            // make sure Ctrl+C can abort editing
            _mainThread = Thread.CurrentThread;
            Console.CancelKeyPress += InterruptEdit;

            SecureString buffer = new SecureString();
            var finished = false;
            while (!finished)
            {
                try
                {
                    finished = ReadKey(buffer);
                }
                catch (ThreadAbortException)
                {
                    // thrown on Ctrl+C
                    Thread.ResetAbort();
                    buffer.Clear();
                    buffer = null;
                    break;
                }
            }

            Console.WriteLine();
            // reset Ctrl+C handling
            Console.CancelKeyPress -= InterruptEdit;
            return buffer;
        }

        private bool ReadKey(SecureString buffer)
        {
            ConsoleKeyInfo i = Console.ReadKey(true);
            if (i.Key == ConsoleKey.Enter)
            {
                return true;
            }
            else if (i.Key == ConsoleKey.Backspace)
            {
                if (buffer.Length > 0)
                {
                    buffer.RemoveAt(buffer.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                buffer.AppendChar(i.KeyChar);
                Console.Write("*");
            }
            return false;
        }

        void InterruptEdit(object sender, ConsoleCancelEventArgs a)
        {
            // Do not abort our program
            a.Cancel = true;
            // ThreadAbortException will be thrown
            _mainThread.Abort();
        }
    }
}

