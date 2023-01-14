namespace TYTDLPCS.Common;

public class PushPullStream : Stream
{
    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length { get; }
    public override long Position { get; set; }
    
    public override void Flush()
    {
        Console.WriteLine("Flush");
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        Console.WriteLine("Read");
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        Console.WriteLine("Seak");
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        Console.WriteLine("SetLength");
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        Console.WriteLine("Write");
        throw new NotImplementedException();
    }
}