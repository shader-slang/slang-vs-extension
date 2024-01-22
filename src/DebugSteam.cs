using System.IO;
using System.Text;

namespace SlangClient
{
    public class DebugStream : Stream
    {
        Stream theStream = null;
        public DebugStream(Stream _theStream)
        {
            theStream = _theStream;
        }

        public override bool CanRead => theStream.CanRead;

        public override bool CanSeek => theStream.CanSeek;

        public override bool CanWrite => theStream.CanWrite;

        public override long Length => theStream.Length;

        public override long Position { get => theStream.Position; set => theStream.Position = value; }

        public override void Flush()
        {
            theStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytes = theStream.Read(buffer, offset, count);

            //string str = Encoding.UTF8.GetString(buffer, 0, count);
            //Debug.WriteLine("Stream Read\n");
            //Debug.WriteLine("-------------------\n");
            //Debug.WriteLine(str);
            //Debug.WriteLine("-------------------\n");
            return bytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return theStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            theStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //long ticks = DateTime.Now.Ticks;
            string str = Encoding.UTF8.GetString(buffer, offset, count);
            //Debug.WriteLine(ticks.ToString() + " Stream Write\n");
            //Debug.WriteLine(ticks.ToString() + " -------------------\n");
            //Debug.WriteLine(ticks.ToString() + str);
            //Debug.WriteLine(ticks.ToString() + " ------------------\n");

            if (str.Contains("NotificationReceived"))
            {
                // Hack to avoid VS2022 send NotificationReceived messages to server which doesn't process them and client restarts the server.
                return;
            }

            theStream.Write(buffer, offset, count);
        }
    }
}

